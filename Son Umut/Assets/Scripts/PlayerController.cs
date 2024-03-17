using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
   private CharacterController controller;
   private AudioSource source;
   
   
   private Vector3 velocity;
   private bool isGrounded;
   private bool isMoving;
   private bool isNoteOpen = false;

   public Transform ground;
   public float distance = 0.3f;

   public float orginalHeight;
   public float crouchHeight;
   

   public float speed;
   public float jumpHeight;
   public float gravity;

   public AudioClip[] FootStepSounds;
   public LayerMask mask;

   float timer;
   public float timeBetweenSteps;

   private void Start()
   {
      controller = GetComponent<CharacterController>();
      source = GetComponent<AudioSource>();
   }

   private void Update()
   {
      //movement Bölgesinin içinde Karakterin Hareket kontrolleri bulunmakta
      #region Movement 
      float horizontal = Input.GetAxis("Horizontal");
      float vertical = Input.GetAxis("Vertical");

      Vector3 move = transform.right * horizontal + transform.forward * vertical;
      controller.Move(move * speed * Time.deltaTime);
      #endregion

      #region FootSteps

      if (horizontal !=0 || vertical !=0)
      {
         isMoving = true;
      }
      else
      {
         isMoving = false;
      }

      if (isMoving)
      {
         timer -= Time.deltaTime;
         if (timer <= 0)
         {
            timer = timeBetweenSteps;
            source.clip = FootStepSounds[Random.Range(0,FootStepSounds.Length)];
            source.Play();
         }
      }
      else
      {
         timer = timeBetweenSteps;
      }

      #endregion
      //jump içinde karakterin zıplama fonksiyonu ve kontrolü bulunmakta
      #region Jump

      if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
      {
         velocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
      }
      #endregion
      //Gravity İçinde yerçekimi fonksiyonu bulunmakta Buraya Dokunmayınız!!!
      #region Gravity

      isGrounded = Physics.CheckSphere(ground.position, distance, mask);
      if (isGrounded && velocity.y < 0)
      {
         velocity.y = 0f;
      }

      velocity.y += gravity * Time.deltaTime;
      controller.Move(velocity * Time.deltaTime);

      #endregion
      //Basit Çömelme fonksiyonu gravity kullanılarak eğilme hızı ayarlanmıştır.
      #region basic crouch

      if (Input.GetKeyDown(KeyCode.LeftControl))
      {
         controller.height = crouchHeight;
         gravity = -2000f;
         speed = 2.0f;
         jumpHeight = 0f;
      }

      if (Input.GetKeyUp(KeyCode.LeftControl))
      {
         controller.height = orginalHeight;
         gravity = -9.8f;
         speed = 5.0f;
         jumpHeight = 1.0f;
      }

      #endregion 
      //Basit Koşma Sadece hızı arttırdım
      #region basic running

      if (Input.GetKeyDown(KeyCode.LeftShift))
      {
         speed = 7.0f;
         timeBetweenSteps = 0.3f;
      }

      if (Input.GetKeyUp(KeyCode.LeftShift))
      {
         speed = 5.0f;
         timeBetweenSteps = 0.5f;
      }

      #endregion
      //note okuma sistemi burada çalışıyor
      #region RaycastNoteReader

      if (Input.GetKeyDown(KeyCode.E))
      {
         Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
         RaycastHit hit;
         if (Physics.Raycast(ray, out hit))
         { 
            if (hit.collider.TryGetComponent(out NoteItem note))
            {
               NoteUIController.Instance.setText(note.getNote());
               isNoteOpen = true; 
            }
         }
      }

      if (Input.GetKeyDown(KeyCode.Escape)&& isNoteOpen)
      {
         NoteUIController.Instance.closeNote();
      }

      #endregion
      
      
      
      
   }
   
}
