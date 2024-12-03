using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringBoard : MonoBehaviour
{

  
  
        public float springForce = 10f; 

        void OnTriggerEnter2D(Collider2D other)
        {
        
                Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.AddForce(Vector2.up * springForce, ForceMode2D.Impulse);
                }
           
        }
    }
