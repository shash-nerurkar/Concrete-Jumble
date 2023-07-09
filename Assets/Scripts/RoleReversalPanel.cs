using UnityEngine;
using TMPro;

public class RoleReversalPanel : MonoBehaviour
{
    public HUD hud;
    
    public Animator animator;

    public TextMeshProUGUI newRoleLabel;

    public void ReverseRoles () {
        Time.timeScale = 0;

        Player player = FindObjectOfType<Player> ();
        switch ( player.Entity.State ) {
            case EntityState.Attacking:
            case EntityState.Defending:
                newRoleLabel.text = "Goon";
                break;
            
            case EntityState.Refereeing:
                newRoleLabel.text = "Referee";
                break;
            
            case EntityState.Held:
                newRoleLabel.text = "Boomerang";
                break;
        }

        hud.volumeButton.gameObject.SetActive ( true );
        gameObject.SetActive ( true );

        animator.SetBool ( "isReversing", true );
    }

    public async void OnRoleReversalAnimationDone () {
        await System.Threading.Tasks.Task.Delay ( millisecondsDelay: 500 );

        Time.timeScale = 1;

        gameObject.SetActive ( false );
        hud.volumeButton.gameObject.SetActive ( true );

        animator.SetBool ( "isReversing", false );
    }
}
