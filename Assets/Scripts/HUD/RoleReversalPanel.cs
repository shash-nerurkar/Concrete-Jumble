using UnityEngine;
using TMPro;

public class RoleReversalPanel : MonoBehaviour
{
    public HUD hud;
    
    public Animator animator;

    public TextMeshProUGUI newRoleLabel;


    public void ReverseRoles ( Entity entity, Entity hitEntity, EntityState hitEntityState, Entity entityCurrentHolder ) {
        Time.timeScale = 0;

        entity.SpriteRenderer.sortingOrder = 2;
        hitEntity.SpriteRenderer.sortingOrder = 2;

        hud.soundsMain.inGameMusic.spatialBlend = 1;

        hud.main.CameraAnimator.Play ( "no_move" );
        hud.main.ParticleSystem.gameObject.SetActive ( false );

        Player player = FindObjectOfType<Player> ();
        switch ( player.Entity.State ) {
            case EntityState.Attacking:
            case EntityState.Defending:
                newRoleLabel.text = "Thug";
                break;
            
            case EntityState.Refereeing:
                newRoleLabel.text = "Referee";
                break;
            
            case EntityState.Held:
                newRoleLabel.text = "Boomerang";
                break;
        }

        gameObject.SetActive ( true );
        hud.volumeButton.gameObject.SetActive ( true );

        animator.SetBool ( "isReversing", true );

        this.entity = entity;
        this.hitEntity = hitEntity;
        this.hitEntityState = hitEntityState;
        this.entityCurrentHolder = entityCurrentHolder;
    }

    private Entity entity;
    private Entity hitEntity;
    private EntityState hitEntityState;
    private Entity entityCurrentHolder;

    public void OnRoleReversalAnimationDone () {
        Time.timeScale = 1;

        entity.SpriteRenderer.sortingOrder = 0;
        hitEntity.SpriteRenderer.sortingOrder = 0;

        hud.soundsMain.inGameMusic.spatialBlend = 0;

        hud.main.CameraAnimator.Play ( "idle" );
        hud.main.ParticleSystem.gameObject.SetActive ( true );

        gameObject.SetActive ( false );
        hud.volumeButton.gameObject.SetActive ( true );

        animator.SetBool ( "isReversing", false );

        entity.OnRoleReversalDone ( hitEntity: hitEntity, hitEntityState: hitEntityState, hitCurrentHolder: entityCurrentHolder );
    }
}
