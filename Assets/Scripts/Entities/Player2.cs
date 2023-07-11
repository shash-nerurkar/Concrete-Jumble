using UnityEngine;

public class Player2 : Hittable
{
    public Entity Entity;
    public Timer attackTimer;

    private Vector3 destinationPosition;


    void Awake () {
        destinationPosition = transform.position;
    }

    void OnAttackTimerFinish () {
        switch ( Entity.State ) {
            case EntityState.Attacking:
                if ( transform.position.y > -Main.ScreenHeightHalved && transform.position.y < ( Main.ScreenHeightHalved * 0.66f ) - 0.5f )
                    Entity.Throw (
                        startDirection: Mathf.Abs ( Entity.main.player.transform.position.y - transform.position.y ) < 0.3f 
                            ? Vector2.zero
                            : new Vector2 ( 0, Mathf.Sign ( Entity.main.player.transform.position.y - transform.position.y ) )
                    );
                
                destinationPosition = new Vector2 ( transform.position.x, Random.Range ( -Main.ScreenHeightHalved, ( Main.ScreenHeightHalved * 0.66f ) - 0.5f ) );

                break;   

            case EntityState.Refereeing:
                destinationPosition = new Vector2 ( transform.position.x, Random.Range ( Main.ScreenHeightHalved * 0.66f, Main.ScreenHeightHalved ) );

                break;
            
            case EntityState.Defending:
            case EntityState.Held:
            default:
                break;        
        }
    }

    void FixedUpdate () {
        switch ( Entity.State ) {
            case EntityState.Attacking:
                if ( !attackTimer.IsRunning || attackTimer.TimeRemaining == 0 )
                    attackTimer.StartTimer ( maxTime: Random.Range ( 0, 2 ), onTimerFinish: OnAttackTimerFinish );
                
                if ( Vector2.Distance ( transform.position, destinationPosition ) > 0.1f ) {
                    Entity.Velocity = ( destinationPosition - transform.position ).normalized * Time.deltaTime;

                    transform.position = new Vector3 (
                        transform.position.x,
                        transform.position.y + Entity.Velocity.y * Entity.MoveSpeed, 
                        0
                    );
                }

                break;

            case EntityState.Refereeing:
                transform.position = new Vector3 (
                    transform.position.x,
                    Main.ScreenHeightHalved, 
                    0
                );

                break;
            
            case EntityState.Defending:
                if ( Vector2.Distance ( transform.position, destinationPosition ) <= 0.1f ) {
                    if ( destinationPosition.y < 0 )
                        destinationPosition = new Vector2 ( transform.position.x, Main.ScreenHeightHalved );
                    else
                        destinationPosition = new Vector2 ( transform.position.x, - Main.ScreenHeightHalved );
                }

                Entity.Velocity = ( destinationPosition - transform.position ).normalized * Time.deltaTime;
                
                transform.position = new Vector3 (
                    transform.position.x,
                    transform.position.y + Entity.Velocity.y * Entity.MoveSpeed, 
                    0
                );

                break;
            
            case EntityState.Held:
            default:

                break;        
        }
    }
}
