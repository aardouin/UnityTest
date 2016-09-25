using UnityEngine;
using System.Collections;

public class KnifeController : MonoBehaviour
{

    // Use this for initialization=
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.LogError("Collision enter");
        Rigidbody2D rigidBody = GetComponent< Rigidbody2D > ();
        if(rigidBody != null)
        {
            rigidBody.velocity = Vector3.zero;
            Destroy(rigidBody);
            GetComponent<Animator>().SetBool("isPlanted", true);
        }
    }


}
