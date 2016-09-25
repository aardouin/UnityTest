using System;
using UnityEngine;

namespace UnityStandardAssets._2D
{
    public class PlatformerCharacter2D : MonoBehaviour
    {
        [SerializeField] private float m_MaxSpeed = 10f;                    // The fastest the player can travel in the x axis.
        [SerializeField] private float m_JumpForce = 400f;                  // Amount of force added when the player jumps.
        [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
        [SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character

        private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
        const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
        private bool m_Grounded;            // Whether or not the player is grounded.
        private Transform m_CeilingCheck;   // A position marking where to check for ceilings
        const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
        private Animator m_Anim;            // Reference to the player's animator component.
        private Transform m_KnifeThrower;
        private Transform m_WallJumpChecker;
        private Rigidbody2D m_Rigidbody2D;
        private bool m_FacingRight = true;  // For determining which way the player is currently facing.

        public Rigidbody2D knife;
        public int maxKnifeCount;
        public  int knifeCount;
        private void Awake()
        {
            // Setting up references.
            m_GroundCheck = transform.Find("GroundCheck");
            m_CeilingCheck = transform.Find("CeilingCheck");
            m_KnifeThrower = transform.Find("KnifeThrower");
            m_WallJumpChecker = transform.Find("WallJumpCheck");
            m_Anim = GetComponent<Animator>();
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
        }


        private void FixedUpdate()
        {
            m_Grounded = false;

            // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
            // This can be done using layers instead but Sample Assets will not overwrite your project settings.
            Collider2D collider2D = gameObject.GetComponent<Collider2D>();
            float height = collider2D.bounds.size.y;

            Collider2D[] colliders = Physics2D.OverlapBoxAll(m_GroundCheck.position, new Vector2(k_CeilingRadius,height), m_WhatIsGround);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                    m_Grounded = true;
            }
            m_Anim.SetBool("Ground", m_Grounded);

            // Set the vertical animation
            m_Anim.SetFloat("vSpeed", m_Rigidbody2D.velocity.y);
        }


        public void OnCollisionEnter2D(Collision2D collision)
        {

            if (collision.gameObject.tag == "CollectibleKnife" && knifeCount != maxKnifeCount)
            {
                knifeCount++;
                Destroy(collision.gameObject);
            }
            
        }
        public void Move(float move, bool crouch, bool jump)
        {
            // If the player should jump...
            Collider2D collider2D = gameObject.GetComponent<Collider2D>();
            float height = collider2D.bounds.size.y;
            Debug.Log("height : "+height);
            bool grabbingWall = Physics2D.OverlapBox(m_WallJumpChecker.position, new Vector2(k_GroundedRadius,height), m_WhatIsGround);

            if ((m_Grounded || grabbingWall) && jump)
            {
                // Add a vertical force to the player.
                m_Grounded = false;
                m_Anim.SetBool("Ground", false);
                m_Rigidbody2D.AddForce(new Vector2(0, m_JumpForce));
            }

            // If crouching, check to see if the character can stand up
            if (!crouch && m_Anim.GetBool("Crouch"))
            {
                // If the character has a ceiling preventing them from standing up, keep them crouching
                if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
                {
                    crouch = true;
                }
            }

            // Set whether or not the character is crouching in the animator
            m_Anim.SetBool("Crouch", crouch);

            //only control the player if grounded or airControl is turned on
            if (m_Grounded || m_AirControl)
            {
                // Reduce the speed if crouching by the crouchSpeed multiplier
                move = crouch ? move*m_CrouchSpeed : move;
                
                bool flipped = false;
                if (move > 0 && !m_FacingRight || move < 0 && m_FacingRight)
                {
                    // ... flip the player.
                    Flip();
                    flipped = true;
                }

                if( grabbingWall && !flipped)
                {
                    move = 0;
                }
                // The Speed animator parameter is set to the absolute value of the horizontal input.
                m_Anim.SetFloat("Speed", Mathf.Abs(move));

                // Move the character
                m_Rigidbody2D.velocity = new Vector2(move * m_MaxSpeed, m_Rigidbody2D.velocity.y);
            }
         
        }

        public void ThrowKnife()
        {
            if( knifeCount > 0)
            {
                Rigidbody2D aKnife = Instantiate(knife, m_KnifeThrower.position, Quaternion.Euler(new Vector3(0, 0, m_FacingRight ? 0 : 180))) as Rigidbody2D;
                aKnife.velocity = new Vector2(m_FacingRight ? 20 : -20, 0);
                knifeCount--;
            }
            
        }


        private void Flip()
        {
            // Switch the way the player is labelled as facing.
            m_FacingRight = !m_FacingRight;

            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }
}
