using System.Collections.Generic;
using UnityEngine;


namespace Viva
{


    public class SocialBehavior : ActiveBehaviors.ActiveTask
    {

        protected class SocialLink
        {
            public readonly Character character;
            public bool greeted = false;
            public float lastInteraction;

            public SocialLink(Character _character)
            {
                character = _character;
                lastInteraction = Time.time;
            }
        }

        private List<SocialLink> socialLinks = new List<SocialLink>();
        private bool reachedSocialCircle = false;
        private Companion.Animation waveAnimation = Companion.Animation.NONE;
        private Companion.Animation targetSocialAnimation = Companion.Animation.NONE;
        private SocialLink currentWaveTarget = null;
        private bool playRandomSocialAnim = true;
        private float returnToEmploymentTimer = 0.0f;


        public SocialBehavior(Companion _self) : base("Socializing", _self, ActiveBehaviors.Behavior.SOCIAL, null)
        {
        }

        // private SocialLink FindSocialLink( Character character ){
        // 	foreach( var info in socialLinks ){
        // 		if( info.character == character ){
        // 			return info;
        // 		}
        // 	}
        // 	return null;
        // }

        // public bool AttemptSocialContact( Character character ){
        // 	if( character == null || character == self ){
        // 		return false;
        // 	}
        // 	var result = FindSocialLink( character );
        // 	if( result != null ){
        // 		if( Time.time-result.lastInteraction < 60.0f ){
        // 			return false;
        // 		}else{
        // 			result.lastInteraction = Time.time;
        // 			result.greeted = false;
        // 		}
        // 	}else{
        // 		socialLinks.Add( new SocialLink( character ) );
        // 	}
        // 	GrowSocialCircle( character );
        // 	reachedSocialCircle = false;

        // 	self.active.SetTask( this, false );
        // 	return true;
        // }

        // private void GrowSocialCircle( Character character ){
        // 	foreach( var info in socialLinks ){
        // 		Companion companion = info.character as Companion;
        // 		info.lastInteraction = Time.time;
        // 		if( companion ){
        // 			companion.active.social.AttemptSocialContact( character );
        // 		}
        // 	}
        // 	returnToEmploymentTimer = 20.0f;
        // }

        // public override void OnActivate(){
        // 	waveAnimation = Companion.Animation.NONE;
        // 	currentWaveTarget = null;
        // 	playRandomSocialAnim = false;
        // 	self.employment.Pause();

        //     self.AddOnNewVisibleItemCallback( OnNewItemVisible );
        // }

        // public override void OnDeactivate(){
        //     self.RemoveOnNewVisibleItemCallback( OnNewItemVisible );
        // 	self.employment.Resume();
        // }

        // private void OnNewItemVisible( Item item ){
        //     if( item.settings.itemType == Item.Type.CHARACTER ){
        //         AttemptSocialContact( item.mainOwner );
        //     }
        // }

        // public override void OnFixedUpdate(){

        // 	if( socialLinks.Count == 0 ){
        // 		self.active.SetTask( self.active.idle, false );
        // 		return;
        // 	}
        // 	if( reachedSocialCircle ){
        // 		FixedUpdateSocialLinks();
        // 	}else{
        // 		GoToSocialCircle();
        // 	}
        // }

        // private void FixedUpdateSocialLinks(){
        // 	foreach( var info in socialLinks ){
        // 		if( !info.greeted ){
        // 			WaveAtCharacter( info );
        // 			return;
        // 		}
        // 	}
        // 	if( playRandomSocialAnim ){
        // 		var lolis = new List<Companion>();
        // 		foreach( var info in socialLinks ){
        // 			Companion companion = info.character as Companion;
        // 			if( companion ){
        // 				lolis.Add( companion );
        // 			}
        // 		}
        // 		if( lolis.Count > 0 ){
        // 			int randomLoliIndex = UnityEngine.Random.Range( 0, lolis.Count );
        // 			var randomLoli = lolis[ randomLoliIndex ];
        // 			self.SetRootFacingTarget( randomLoli.floorPos, 300.0f, 30.0f, 20.0f );
        // 			self.SetLookAtTarget( randomLoli.head );
        // 			self.SetViewAwarenessTimeout( 1.0f );
        // 		}
        // 		targetSocialAnimation = GetAvailableSocialAnimation( lolis.Count > 0 );
        // 		if( targetSocialAnimation != Companion.Animation.NONE ){
        // 			playRandomSocialAnim = false;
        // 		}
        // 	}else{
        // 		if( targetSocialAnimation != Companion.Animation.NONE && self.currentAnim == Companion.Animation.STAND_HAPPY_IDLE1 ){
        // 			self.SetTargetAnimation( targetSocialAnimation );
        // 			//pick random character and face them
        // 		}
        // 		self.active.idle.CheckForVisibleNewInterests();
        // 	}

        // 	returnToEmploymentTimer -= Time.deltaTime;
        // 	if( returnToEmploymentTimer < 0.0f ){
        // 		self.active.SetTask( self.active.idle, true );
        // 		//make others return as well
        // 		foreach( var info in socialLinks ){
        // 			Companion companion = info.character as Companion;
        // 			info.lastInteraction = Time.time;
        // 			if( companion && companion.active.IsTaskActive( companion.active.social ) && companion.employment.isActive ){
        // 				companion.active.SetTask( companion.active.idle, true );
        // 			}
        // 		}
        // 	}
        // }

        // private Companion.Animation GetAvailableSocialAnimation( bool allowSocialAnim ){
        // 	var idleAnim = self.active.idle.GetAvailableIdleAnimation();
        // 	if( idleAnim != Companion.Animation.NONE && ( UnityEngine.Random.value > 0.5f || !allowSocialAnim ) ){
        // 		return idleAnim;
        // 	}else{
        // 		if( allowSocialAnim ){
        // 			return Companion.Animation.STAND_HAPPY_SOCIAL1;
        // 		}else{
        // 			return Companion.Animation.STAND_HAPPY_IDLE3;
        // 		}
        // 	}
        // }

        // private void GoToSocialCircle(){

        // 	Vector3 circleCenter = Vector3.zero;
        // 	foreach( var info in socialLinks ){
        // 		circleCenter += info.character.floorPos;
        // 	}
        // 	circleCenter /= socialLinks.Count;

        // 	bool reachedDestination = self.active.follow.AttemptFollowRefresh( circleCenter, true, 1.2f );
        // 	if( reachedDestination ){
        // 		reachedSocialCircle = true;
        // 	}else{
        // 		self.SetTargetAnimation( Companion.Animation.STAND_GIDDY_LOCOMOTION );
        // 	}
        // }

        // private void WaveAtCharacter( SocialLink info ){
        // 	//wait for last target
        // 	if( currentWaveTarget != null ){
        // 		return;
        // 	}
        // 	//wait to be fully idle
        // 	if( !self.IsCurrentAnimationIdle() ){
        // 		return;
        // 	}
        // 	float bearing = Tools.Bearing( self.transform, info.character.floorPos );
        // 	if( Mathf.Abs( bearing ) > 20.0f ){
        //         self.SetRootFacingTarget( info.character.floorPos, 200.0f, 20.0f, 20.0f );
        // 		return;
        // 	}
        // 	Companion companion = info.character as Companion;
        // 	//wait for companion to gain balance to asy hello
        // 	if( companion && !companion.hasBalance ){
        // 		return;
        // 	}
        // 	waveAnimation = self.active.idle.GetAvailableWaveAnimation();
        // 	Debug.Log(waveAnimation);
        //     if( waveAnimation != Companion.Animation.NONE ){
        //         self.SetTargetAnimation( waveAnimation );
        // 		currentWaveTarget = info;
        // 		self.SetLookAtTarget( info.character.head );
        // 		self.SetViewAwarenessTimeout( 2.0f );
        //     }
        // }

        // private void OnSuccessfulGreet( SocialLink info ){
        // 	Companion companion = info.character as Companion;
        // 	info.greeted = true;
        // 	if( companion ){
        // 		if( companion.employment.isActive ){
        // 			companion.locomotion.StopMoveTo();
        // 			companion.active.social.AttemptSocialContact( self );
        // 			companion.SetRootFacingTarget( self.spine1.position, 300.0f, 30.0f, 20.0f );
        // 		}	///TODO CALLBACK FOR ALLOWED EMPLOYMENT TASKS
        // 	}
        // 	playRandomSocialAnim = true;
        // }

        // public override void OnAnimationChange( Companion.Animation oldAnim, Companion.Animation newAnim ){
        // 	if( newAnim == waveAnimation ){
        // 		if( currentWaveTarget != null ){
        // 			OnSuccessfulGreet( currentWaveTarget );
        // 			currentWaveTarget = null;
        // 		}
        // 	}else if( newAnim == targetSocialAnimation ){
        // 		playRandomSocialAnim = true;	//resume
        // 	}
        // }
    }

}