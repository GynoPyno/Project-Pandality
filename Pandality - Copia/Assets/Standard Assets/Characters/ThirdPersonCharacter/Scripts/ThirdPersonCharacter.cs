using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]

	public class ThirdPersonCharacter : MonoBehaviour
	{
		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;
		[SerializeField] float m_JumpPower = 12f;
		[Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;

		[SerializeField] float m_RunCycleLegOffset = 0.2f; 
		//specific to the character in sample assets, will need to be modified to work with others

		[SerializeField] float m_MoveSpeedMultiplier = 1f;
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		[SerializeField] float m_GroundCheckDistance = 0.1f;

		Rigidbody m_Rigidbody;
		Animator m_Animator;
		bool m_IsGrounded;
		float m_OrigGroundCheckDistance;
		const float k_Half = 0.5f;
		float m_TurnAmount;
		float m_ForwardAmount;
		Vector3 m_GroundNormal;
		float m_CapsuleHeight;
		Vector3 m_CapsuleCenter;
		CapsuleCollider m_Capsule;
		bool m_Crouching;
		bool DJump;		//var che gestisce i doppisalti
		static private short nocc = 0;
		static private short chiavi = 0;
		static private short vite = 5;
		static private bool morte = false;

		void Start()
		{
			m_Animator = GetComponent<Animator>();
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Capsule = GetComponent<CapsuleCollider>();
			m_CapsuleHeight = m_Capsule.height;
			m_CapsuleCenter = m_Capsule.center;

			m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;	// "blocca" la rotazione del rigidbody (?)
			m_OrigGroundCheckDistance = m_GroundCheckDistance;
          
		}


		public bool Move(Vector3 move, bool crouch, bool jump)
		{

			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f) move.Normalize();	//serve per limitare la velocità di movimento
			move = transform.InverseTransformDirection(move);
			CheckGroundStatus();
			move = Vector3.ProjectOnPlane(move, m_GroundNormal);
			m_TurnAmount = Mathf.Atan2(move.x, move.z);
			m_ForwardAmount = move.z;

			ApplyExtraTurnRotation();

			// control and velocity handling is different when grounded and airborne:
			if (m_IsGrounded)
			{                
               	DJump = HandleGroundedMovement(crouch, jump, DJump); 	//controlla il movimento a terra
			}
			else
			{                
				DJump = HandleAirborneMovement(jump,DJump);		//controlla il movimento in aria
			}
            
			ScaleCapsuleForCrouching(crouch);		//ridimensiona il collider(?) per quando si è accovacciati
			PreventStandingInLowHeadroom();			//impedisce di stare in piedi dove non puoi

			// send input and other state parameters to the animator
			UpdateAnimator(move);
			return morte;
		}


		void ScaleCapsuleForCrouching(bool crouch)
		{
			if (m_IsGrounded && crouch)
			{
				if (m_Crouching) return;
				m_Capsule.height = m_Capsule.height / 2f;
				m_Capsule.center = m_Capsule.center / 2f;
				m_Crouching = true;
			}
			else
			{
				Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength))
				{
					m_Crouching = true;
					return;
				}
				m_Capsule.height = m_CapsuleHeight;
				m_Capsule.center = m_CapsuleCenter;
				m_Crouching = false;
			}
		}

		void PreventStandingInLowHeadroom()
		{
			// prevent standing up in crouch-only zones
			if (!m_Crouching)
			{
				Ray crouchRay = new Ray(m_Rigidbody.position + Vector3.up * m_Capsule.radius * k_Half, Vector3.up);
				float crouchRayLength = m_CapsuleHeight - m_Capsule.radius * k_Half;
				if (Physics.SphereCast(crouchRay, m_Capsule.radius * k_Half, crouchRayLength))
				{
					m_Crouching = true;
				}
			}
		}


		void UpdateAnimator(Vector3 move)	//gli passo un vettore movimento
		{
			// update the animator parameters
			m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
			//Debug.Log ("Forward amount->" + m_Animator.GetFloat ("Forward"));
			m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
			m_Animator.SetBool("Crouch", m_Crouching);
			m_Animator.SetBool("OnGround", m_IsGrounded);

			if (!m_IsGrounded)	//controlla che NON sia a terra
			{
				m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);	
				//Debug.Log("m_Rigidbody.velocity.y->" + m_Rigidbody.velocity.y);
			}

			// calculate which leg is behind, so as to leave that leg trailing in the jump animation
			// (This code is reliant on the specific run cycle offset in our animations,
			// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
			float runCycle =
				Mathf.Repeat(
					m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
			float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;

			if (m_IsGrounded)
			{
				m_Animator.SetFloat("JumpLeg", jumpLeg);
			}

			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (m_IsGrounded && move.magnitude > 0)
			{
				m_Animator.speed = m_AnimSpeedMultiplier;
			}
			else
			{
				// don't use that while airborne
				m_Animator.speed = 1;
			}
		}


		bool HandleAirborneMovement(bool jump, bool DJump)	//funzione che controlla il personaggio a mezzaria
		{													//la variabile DJump serve per controllare se il personaggio ha già eseguito un doppio salto

			if (jump && DJump)	//se l'utente preme "spazio" e il personaggio può eseguire il doppiosalto allora entra nell'if
            {
                // jump!
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
                
                m_Animator.applyRootMotion = false;
                m_GroundCheckDistance = 0.1f;
				DJump = false;	//fino a quando non ritorna a terrà non può più saltare
            }

			// apply extra gravity from multiplier:
			Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
			m_Rigidbody.AddForce(extraGravityForce);

			m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
			return DJump;	//ritorno la variabile aggiornata
		}

		bool HandleGroundedMovement(bool crouch, bool jump, bool DJump)	//la variabile DJump serve per controllare se il personaggio ha già 
		{																//eseguito un doppio salto
		


        	// controlla che si possa fare un salto
			if (jump && !crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))	//l'ultima parte serve per sapere se il
			{																						//personaggio sta toccando terra
			
			// jump!
				m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
				m_IsGrounded = false;
                m_Animator.applyRootMotion = false;
				m_GroundCheckDistance = 0.1f;
				DJump = true;	//ora il personaggio può eseguire un doppio salto			
			}
			return DJump;	//ritorno la variabile aggiornata
		}

		void ApplyExtraTurnRotation()			
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
			transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
		}


		public void OnAnimatorMove()
		{
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied. 
			if (m_IsGrounded && Time.deltaTime > 0)
			{
				//Debug.Log("m_Animator.deltaPosition->" + m_Animator.deltaPosition);
				Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

				// we preserve the existing y part of the current velocity.
				v.y = m_Rigidbody.velocity.y;
				m_Rigidbody.velocity = v;
				//Debug.Log("m_Rigidbody.velocity->" + m_Rigidbody.velocity);
			}
		}

		//questo metodo verrà richiamato all'entrata di un trigger
		public void Controlla_Noccioline_E_Chiavi(bool ID)
		{
			//ID-> se verà il gocatore ha trovato una nocciloina, altrimenti ha trovato una chiave
			if (ID) 
			{

				if (nocc != 5)	//controllo se ha già trovato tutte le noccioline ---> controllo provvisorio  
				{
					nocc++;
					if (nocc == 5) {
						Debug.Log ("Complimenti hai trovato tutte le noccioline!");
					}

				}
			}else 
			{
				if (chiavi != 5)	//controllo se ha già trovato tutte le chiavi !!DA RMUOVERE!!-> idem come sopra
				{
					chiavi++;
					if (chiavi == 5)
					{
						Debug.Log ("Complimenti hai trovato tutte le chiavi!");
					}
				}
		
			}
			
		}

		//questo metodo verrà richiamato da un evento
		public void vita()	//metodo che gestisce le vite del giocatore
		{
			vite--;
			if (vite <= 0)
			{
				Debug.Log ("Mi dispiace ma sei morto");
				morte = true;
			}
		}





		void CheckGroundStatus()
		{
			RaycastHit hitInfo;
#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
			// 0.1f is a small offset to start the ray from inside the character
			// it is also good to note that the transform position in the sample assets is at the base of the character
			if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
			{
				m_GroundNormal = hitInfo.normal;
				m_IsGrounded = true;
				m_Animator.applyRootMotion = true;
			}
			else
			{
				m_IsGrounded = false;
				m_GroundNormal = Vector3.up;
				m_Animator.applyRootMotion = false;
			}
		}
	}
}
