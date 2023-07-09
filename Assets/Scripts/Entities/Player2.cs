using UnityEngine;

public class Player2 : Hittable
{
    public Entity Entity;

    private Vector3 destinationPosition;
    private Vector3 destinationDirection;

    private Timer attackTimer;

    void Awake () {
        destinationPosition = transform.position;

        attackTimer = gameObject.AddComponent<Timer> ();
    }

    void OnAttackTimerFinish () {
        if ( Random.Range ( 0, 1 ) < 0.5f )
            Entity.Throw ( startDirection: Vector2.zero );

        destinationPosition = new Vector2 ( transform.position.x, Random.Range ( -Main.ScreenHeightHalved, Main.ScreenHeightHalved ) );
    }

    void FixedUpdate () {
        switch ( Entity.State ) {
            case EntityState.Attacking:
                if ( !attackTimer.IsRunning || attackTimer.TimeRemaining == 0 )
                    attackTimer.StartTimer ( maxTime: Random.Range ( 0, 1 ), onTimerFinish: OnAttackTimerFinish );
                
                // print(
                //     "Vector2.Distance ( transform.position, destinationPosition ): " + Vector2.Distance ( transform.position, destinationPosition ) +
                //     " | attackTimer.TimeRemaining: " + attackTimer.TimeRemaining
                // );
                if ( Vector2.Distance ( transform.position, destinationPosition ) <= 1 ) {
                    destinationDirection = ( destinationPosition - transform.position ).normalized;

                    transform.position = new Vector3 (
                        transform.position.x,
                        transform.position.y + destinationDirection.y * Entity.MoveSpeed * Time.deltaTime, 
                        0
                    );
                }
                // Entity.Rigidbody.AddForce ( new Vector2 ( 0, destinationPosition.y * Entity.MoveSpeed ) * Time.deltaTime );
                
                break;
            
            case EntityState.Refereeing:
                if ( Vector2.Distance ( transform.position, destinationPosition ) <= 0.1f ) {
                    destinationPosition = new Vector2 ( transform.position.x, Random.Range ( 0, Main.ScreenHeightHalved ) );
                }

                destinationDirection = ( destinationPosition - transform.position ).normalized;
                
                transform.position = new Vector3 (
                    transform.position.x,
                    transform.position.y + destinationDirection.y * Entity.MoveSpeed * Time.deltaTime, 
                    0
                );
                // Entity.Rigidbody.AddForce ( new Vector2 ( 0, destinationPosition.y * Entity.MoveSpeed ) * Time.deltaTime );

                break;
            
            case EntityState.Defending:
                if ( Vector2.Distance ( transform.position, destinationPosition ) <= 0.1f ) {
                    if ( destinationPosition.y < 0 )
                        destinationPosition = new Vector2 ( transform.position.x, Main.ScreenHeightHalved );
                    else
                        destinationPosition = new Vector2 ( transform.position.x, - Main.ScreenHeightHalved );
                }

                destinationDirection = ( destinationPosition - transform.position ).normalized;
                
                transform.position = new Vector3 (
                    transform.position.x,
                    transform.position.y + destinationDirection.y * Entity.MoveSpeed * Time.deltaTime, 
                    0
                );
                // Entity.Rigidbody.AddForce ( new Vector2 ( 0, destinationPosition.y * Entity.MoveSpeed ) * Time.deltaTime );

                break;
            
            case EntityState.Held:
            default:
                break;
            
        }
    }
}
