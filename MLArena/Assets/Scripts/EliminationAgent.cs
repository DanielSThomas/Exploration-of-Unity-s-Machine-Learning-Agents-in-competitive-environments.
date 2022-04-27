using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UI;

public class EliminationAgent : Agent
{
    private EliminationGameManager eliminationGameManager;

    [SerializeField] int health;
    [SerializeField] bool skipManager = false;
    [SerializeField] private Text healthText;
    [SerializeField] private float speed = 10;
    [SerializeField] private float turnspeed = 10;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D collider2D;
    [SerializeField] private Transform target;
  
    [SerializeField] private float meanReward;
    [SerializeField] private int team; // 0 = Red Team  1 = Blue Team
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private GameObject bulletobject;
    [SerializeField] private Rigidbody2D turretPivot;

    [SerializeField] private float firerate;

    [SerializeField] private bool stunned;
   
    
    private float nextShoot;
    private bool canShoot = true;


     

    // Start is called before the first frame update
    void Awake()
    {
        
     
        if (skipManager == false)
        {
            eliminationGameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<EliminationGameManager>();
            MaxStep = eliminationGameManager.getMaxStep();
        }
            
        
    }

    private void Update()
    {
        
         SetReward(-1);
        

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        healthText.text = health.ToString();

       
    }

    public override void OnEpisodeBegin()
    {
        rb.angularVelocity = 0;
        rb.velocity = Vector2.zero;
        rb.rotation = 0;
        turretPivot.rotation = 0;
        Debug.Log("Episode " + CompletedEpisodes);
        health = 3;
        collider2D.enabled = true;
        stunned = false;
    }



    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(canShoot);
        sensor.AddObservation(stunned);
       
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions

        if(stunned == false)
        {
            Vector2 movedir = new Vector2(0, 0);

            int horizontalDir = actionBuffers.DiscreteActions[0];
            int verticalDir = actionBuffers.DiscreteActions[1];
            int turretRotDir = actionBuffers.DiscreteActions[2];
            int shooting = actionBuffers.DiscreteActions[3];

            switch (horizontalDir)
            {
                case 0: horizontalDir = 0; break;
                case 1: horizontalDir = -10; break;
                case 2: horizontalDir = 10; break;
            }

            switch (verticalDir)
            {
                case 0: movedir.y = 0; break;
                case 1: movedir.y = 1; break;
                case 2: movedir.y = -1; break;
            }

            switch (turretRotDir)
            {
                case 0: turretRotDir = 0; break;
                case 1: turretRotDir = 10; break;
                case 2: turretRotDir = -10; break;
            }


            switch (shooting)
            {
                case 0: break;
                case 1: Shoot(); break;
            }


            rb.MoveRotation(rb.rotation += horizontalDir * turnspeed * Time.deltaTime);

            turretPivot.MoveRotation(turretPivot.rotation += turretRotDir * turnspeed * Time.deltaTime);

            rb.velocity = (Vector2)transform.up * movedir.y * speed * Time.deltaTime;

            //Rewards

            meanReward = GetCumulativeReward();

            //Idle Penalty

            AddReward(-1f / MaxStep);


            if (health < 1)
            {

                stunned = true;

                //Give blue team a score
                if (team == 0)
                {
                    eliminationGameManager.addBlueScore();
                }
                //Give red team a score
                if (team == 1)
                {
                    eliminationGameManager.addRedScore();
                }

                
                
                rb.angularVelocity = 0;
                rb.velocity = Vector2.zero;
                collider2D.enabled = false;
                

                Debug.Log(this.gameObject.name + " Lost with a score of : " + GetCumulativeReward());               
            }
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Bumping into walls penalty
        if (collision.gameObject.tag == "Wall")
        {
            AddReward(-0.3f);
            
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut) // Player Control
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        switch (Input.GetAxisRaw("Horizontal"))
        {
            case -1: discreteActionsOut[0] = 2; break;
            case 0: discreteActionsOut[0] = 0; break;
            case 1: discreteActionsOut[0] = 1; break;
        }

        switch (Input.GetAxisRaw("Vertical"))
        {
            case -1: discreteActionsOut[1] = 2; break;
            case 0: discreteActionsOut[1] = 0; break;
            case 1: discreteActionsOut[1] = 1; break;
        }

        if (Input.GetKey(KeyCode.K))
        {
            discreteActionsOut[2] = 1;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            discreteActionsOut[2] = 2;
        }
        else
        {
            discreteActionsOut[2] = 0;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[3] = 1; 
        }
        else
        {
            discreteActionsOut[3] = 0;
        }


    }

    

    public int getHealth()
    {
        return health;
    }

    public void setHealth(int value)
    {
        health = value;
    }
    private void Shoot()
    {
        if (Time.time > nextShoot)
        {
            nextShoot = Time.time + firerate;

            canShoot = true;
        }

        if(canShoot == true)
        {
            GameObject _bullet = Instantiate(bulletobject, bulletSpawn.position, bulletSpawn.rotation);
            _bullet.GetComponent<Bullet>().setbulletOwner(this);
            _bullet.GetComponent<Bullet>().setbulletTeam(team);

            canShoot = false;
        }

    }

    public void endEpisode()
    {
        this.EndEpisode();
    }

    public int getTeam()
    {
        return team;
    }

    public void setSpawn(Transform _spawn)
    {
        this.transform.localPosition = _spawn.position;
    }

  
   

   


}