using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{

    bool movingRight, movingLeft, jump; //Outputs des Neuronalen Netzes
    Rigidbody2D rd;
    int speed = 5;
    int jumpForce = 15;
    float maxReachedX = 9f;

    bool canJump = true; //Jump Lock

    float deathTime = 4f; //Zeit in der Luft (ohne Bodenkontakt) nach der Player stirbt

    float lastGround = 0f;

    public NeuralNetwork brain;

    public float fitness = 0f;

    float decisionRate = 0.1f; //eine Entscheidung alle 0,1 sekunden
    float nextDecisionTime = 0f;

    float aliveTime;
    float speedFactor = 0.5f; //Faktor wie stark durchschnittsgeschwindigkeit einfluss auf fitness hat

    //Um stehenbleiber zu bestrafen
    float idleThreshold = 1f;   // minimale Bewegung in X-Richtung
    float idleTimeLimit = 3f;
    float lastX = 0f;
    float lastMoveTime;

    //hin und hergerinne bestrafen
    float maxX = 0f;
    float lastThresholdTime;
    float thresholdtimeLimit = 5f;
    int directionChanges = 0;

    //Überprüfen über wieviele Löcher gesprungen wird
    bool attemptedJumpOverHole = false;
    int jumpsOverHole= 0;

    void Start()
    {
        rd = GetComponent<Rigidbody2D>();
        aliveTime = Time.time;
        lastMoveTime = Time.time;
        lastGround = Time.time;
        lastThresholdTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextDecisionTime)
        {
            nextDecisionTime = Time.time + decisionRate;
            BrainMovement();
        }
        Movement();
        LimitLeftMovement();
        KillOnTime();
        KillOnIdle();
        SetMax();
        KillOnNoProgress();
    }

    void Movement()
    {
        float targetX = rd.velocity.x;
        if (movingRight)
        {
            if(rd.velocity.x < 0) { directionChanges++; }
            if (IsGrounded()) { 
                targetX = speed;
            }
        }
        else if (movingLeft)
        {
            if (rd.velocity.x > 0) { directionChanges++; }
            if (IsGrounded())
            {
                targetX = -speed;
            }
        }
        if (jump && IsGrounded() && canJump)
        {
            rd.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canJump = false;   //kein weiterer Sprung bis Boden-Kontakt
            jump = false;
        }
        float smoothedX = Mathf.Lerp(rd.velocity.x, targetX, 0.1f);
        rd.velocity = new Vector2(smoothedX, rd.velocity.y);
    }

    void BrainMovement()
    {
        float[] inputs = GatherInputs();

        if(brain == null) { return; } //Warten bis Brain gesetzt wurde
        float[] outputs = brain.FeedForward(inputs); //0: MovingLeft, 1: MovingRight, 2: Jump

        //Movement
        if (outputs[0] >= 0.5f && outputs[1] < 0.5f) // Move left
        {
            movingRight = false;
            movingLeft = true;
        }
        else if (outputs[1] >= 0.5f && outputs[0] < 0.5f) // Move right
        {
            movingRight = true;
            movingLeft = false;
        }
        else
        {
            movingRight = false;
            movingLeft = false;
        }

        //Jumping
        if (outputs[2] >= 0.5f)
        {
            jump = true;
        }
    }

    void TastaturMovement()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            movingRight = false;
            movingLeft = true;
        }
        else if(Input.GetKeyDown(KeyCode.D))
        {
            movingLeft = false;
            movingRight = true;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
        }
    }

    bool IsGrounded()
    {

        Collider2D hit =Physics2D.OverlapCircle(transform.position + Vector3.down * 0.5f, 0.1f, LayerMask.GetMask("Default"));
        return hit != null;
    }

    //sicherstellen das Player nicht weiter links laufen kann als die Kamera gerade anzeigt um zurückgehen nicht zu erlauben
    void LimitLeftMovement() {

        if(transform.position.x  > maxReachedX)
        {
            maxReachedX = transform.position.x;
        }

        float leftLimit = maxReachedX - 9f; // etwas Spielraum
        if (transform.position.x < leftLimit)
        {
            transform.position = new Vector3(leftLimit, transform.position.y, transform.position.z);
        }
    }

    void KillOnTime()
    {
        if (IsGrounded())
        {
            lastGround = Time.time;
            canJump = true;
        }
        else if (Time.time - lastGround > deathTime) {
            Die();
        }
    }

    void KillOnIdle()
    {
        float moved = Mathf.Abs(transform.position.x - lastX);

        if (moved > idleThreshold)
        {
            lastX = transform.position.x;
            lastMoveTime = Time.time;
        }

        else if (Time.time - lastMoveTime > idleTimeLimit)
        {
            Debug.Log("Idle Kill");
            fitness -= 10;   // Strafe
            Die();
        }
    }

    void KillOnNoProgress()
    {
        if(Time.time - lastThresholdTime > thresholdtimeLimit)
        {
            Debug.Log("No Progress Kill");
            fitness -= 10;
            Die();
        }
    }

    void SetMax()
    {
        if(transform.position.x > maxX)
        {
            maxX = transform.position.x;
            lastThresholdTime = Time.time;
        }
    }


    //Player ist gestorben
    void Die()
    {
        //Fitness an Gehirn übertragen bevor er stirbt
        UpdateFitness();
        Debug.Log("Die with fitness" + fitness);
        brain.SetFitness(fitness);

        // PlayerManager informieren
        PlayerManager.instance.RemovePlayer(this);

        // Physik und Input abschalten
        rd.velocity = Vector2.zero;
        rd.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        // Und dann zerstören 
        Destroy(gameObject);
    }

    float[] GatherInputs()
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position,Vector2.left, 20f, LayerMask.GetMask("HoleTrigger")); //RayCast um letztes Loch zu finden
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 20f, LayerMask.GetMask("HoleTrigger"));//RayCast um nächstes Loch zu finden
        RaycastHit2D hitUnder = Physics2D.Raycast(transform.position, Vector2.down, 10f, LayerMask.GetMask("Default"));//RayCast um Abstand zum Boden zu berechnen

        float distanceR =  hitRight.distance; //Distanz zum nächsten Loch
        float distanceL = hitLeft.distance; //Distanz zum letzten Loch
        float distanceU = hitUnder.distance; //Abstand nach unten
        float grounded = IsGrounded() ? 1: 0; //ob Boden berührt (0 wenn nein, 1 wenn ja)

        float[] inputs = new float[] { distanceL, distanceR, distanceU, grounded };
        return inputs;
    }

    void UpdateFitness()
    {
        fitness = transform.position.x;
        fitness = fitness + speedFactor * (fitness / (Time.time - aliveTime)); //Durchschnittsgeschwindigkeit in Fitness einbauen um unnötiges hin und her laufen zu minimieren
        //Zu häufiges Wechseln der Richtung strafen
        fitness -= directionChanges * 0.5f;
        //Sprünge über Löcher stark belohnen
        fitness += jumpsOverHole * 30f;
    }


    public void SetBrain(NeuralNetwork net)
    {
        brain = net;
    }

    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.GetMask("HoleTrigger"))
        {
            if (collision.gameObject.tag == "Entry")
            {
                attemptedJumpOverHole = true;
            }
            //Erhöhe Sprung wenn durch Entry über Loch ist und durch Exit geht und dabei hinter Exit rauskommt/sprich nicht einfach durch exit runterfällt
            else if (collision.gameObject.tag == "Exit" && this.transform.position.x >= collision.gameObject.transform.position.x)
            {
                if (attemptedJumpOverHole)
                {
                    attemptedJumpOverHole = false;
                    jumpsOverHole++;
                    Debug.Log("Jump Over Hole: " +jumpsOverHole);
                }
            }
        }
    }
}
