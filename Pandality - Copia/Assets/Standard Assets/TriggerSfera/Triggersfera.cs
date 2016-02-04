using UnityEngine;
using System.Collections;

	public class Triggersfera : MonoBehaviour {


		GameObject Player;

		// Use this for initialization
		void LateStart () 
		{
			Player = GameObject.Find ("Player");//trovo il giocatore			
		}

		void OnTriggerEnter(Collider other) 
		{
			Debug.Log ("Premi F1,F2 o F3");
		}

		void OnTriggerStay()//quando il giocatore rimane nel trigger controllo se preme un tasto
		{
			string scelta = "0";
			if(Input.GetKeyDown(KeyCode.F1)) 	// scelta del giocatore
				scelta = "1";
			else if(Input.GetKeyDown(KeyCode.F2))
				scelta = "2";
			else if(Input.GetKeyDown(KeyCode.F3))
				scelta = "3";
			else if(Input.GetKeyDown(KeyCode.F4))
				scelta = "4";

			switch (scelta) {
			case "1":	//applica una forza al personaggio
			{
				Player = GameObject.Find ("Player");//trovo il giocatore
				Rigidbody PlayerRB = Player.GetComponent<Rigidbody> ();	//vado a recuperare il suo rb				
				
				PlayerRB.AddForce (Vector3.forward * 20000f);
				PlayerRB.AddForce(Vector3.up * 2000f);
				break;
			}

			case "2":	//cambia il colore della luce
			{
				float duration = 1F;
				Color color0 = Color.red;
				Color color1 = Color.blue;

				Light lt;

				GameObject Luce = GameObject.Find ("Directional Light");//trova l'oggetto "luce"
				lt = Luce.GetComponent<Light>();

				float t = Mathf.PingPong(Time.time, duration) / duration;
				lt.color = Color.Lerp(color0, color1, t);


				break;
			}
			case "3":
			{

				Player = GameObject.Find ("Player");//trovo il giocatore

				
				Player.SendMessage("Controlla_Noccioline_E_Chiavi",true);

				
				break;
			}
			case "4":
			{
				Player = GameObject.Find ("Player");//trovo il giocatore
				
				
				Player.SendMessage("vita");
				
				
				break;
			}
			default:
				break;
			}

			
		}

	}