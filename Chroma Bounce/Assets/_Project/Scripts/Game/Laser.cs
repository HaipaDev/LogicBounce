using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Sirenix.OdinInspector;

public class Laser : MonoBehaviour{
    [Header("References")]
    [SerializeField] SpriteNColor spritencolor_positive;
    //[SerializeField] Material material_positive;
    [SerializeField] SpriteNColor spritencolor_negative;
    //[SerializeField] Material material_negative;
    [Header("Variables")]
    [SerializeField]public bool positive=true;
    [SerializeField]public bool bouncy=false;
    [SerializeField]float scaleupSpeed=3.5f;
    [SerializeField]Laser laserMirrored;
    [DisableInEditorMode][SerializeField]Laser laserParentReference;
    [DisableInEditorMode][SerializeField]GameObject mirrorReference;
    [DisableInEditorMode][SerializeField]public bool clonedLaser;
    [DisableInEditorMode][SerializeField]public bool otherLaserCollisionOn;
    [DisableInEditorMode][SerializeField]public float otherLaserCollisionDelay;

    SpriteRenderer spr;
    void Start(){
        spr=GetComponent<SpriteRenderer>();
        SetPolarity(positive,true,true);
        _posWhenTouched=transform.position;touchingBottom=false;touchingUp=false;
        if(clonedLaser){
            LevelMapManager.instance.laserListSortedWithCloned.Add(this);
            otherLaserCollisionOn=false;
            otherLaserCollisionDelay=0.1f;
        }
    }
    void Update(){
        // if(otherLaserCollisionDelay>0){
        //     otherLaserCollisionDelay-=Time.deltaTime;
        // }else{otherLaserCollisionOn=true;}
    }
    void FixedUpdate(){
        if(!GameManager.GlobalTimeIsPausedNotStepped){
            AutoScale();
            CheckForCollisions();
            if(otherLaserCollisionDelay>0){
                otherLaserCollisionDelay-=Time.fixedDeltaTime;
            }else{otherLaserCollisionOn=true;}
        }
    }
    bool touchingBottom=false,touchingUp=false;
    float raycastLengthDown=0.2f,raycastLengthUp=0.03f;
    Vector2 raycastDownOrigin,laserDownDirection;
    Vector2 raycastUpOrigin,laserUpDirection;
    Vector2 _posWhenTouched;
    void AutoScale(){
        if(!touchingUp){MoveUp();}//Move upwards till hits a wall
        if(touchingUp&&!touchingBottom){ScaleDownwards();}//Scale up downwards
        if(!touchingUp&&!touchingBottom&&!clonedLaser){//Move upwards & scale down
            MoveUp(true);
            ScaleDownwards();
        }
        if(touchingBottom&&!touchingUp&&!clonedLaser){ScaleBackDown();}//Scale down if touching bottom but not up
        void MoveUp(bool slowerrate=false){
            float _step=scaleupSpeed*Time.fixedDeltaTime;if(slowerrate){_step=(scaleupSpeed/2)*Time.fixedDeltaTime;}
            Vector2 moveUpwards=laserUpDirection.normalized*_step;
            transform.Translate(moveUpwards);
        }
        void ScaleDownwards(){
            float step=scaleupSpeed*Time.fixedDeltaTime;
            transform.localScale=new Vector2(transform.localScale.x,transform.localScale.y+step);
        }
        void ScaleBackDown(){
            float step=scaleupSpeed*Time.fixedDeltaTime;
            transform.localScale=new Vector2(transform.localScale.x,transform.localScale.y-step);
        }


        if((Vector2)transform.position!=_posWhenTouched&&_posWhenTouched!=Vector2.zero||transform.localScale.y>12f){//Reset when position changed or too big (outside walls) [12 is the maximum diagonal]
            transform.localScale=new Vector2(transform.localScale.x,0.1f);_posWhenTouched=Vector2.zero;
        }
        if(touchingUp&&touchingBottom&&transform.localScale.y<0.15f){//Push out of the walls
            float _step=scaleupSpeed*Time.fixedDeltaTime;
            Vector2 moveUpwards=laserUpDirection.normalized*_step;
            transform.Translate(-moveUpwards);
        }
        if(clonedLaser&&laserParentReference==null||(laserParentReference!=null&&!laserParentReference.gameObject.activeSelf)){Destroy(gameObject);}
        //if(laserParentReference!=null)if(laserParentReference.mirrorReference!=null){Debug.Log(Vector2.Distance(transform.position,laserParentReference.mirrorReference.transform.position));if(Vector2.Distance(transform.position,laserParentReference.mirrorReference.transform.position)>=0.55f)Destroy(gameObject);}//If falls through destroy
    }
    void CheckForCollisions(){
        laserDownDirection = Quaternion.Euler(0f,0f,transform.eulerAngles.z) * Vector2.down;
        float _correction=0;
        if(transform.eulerAngles.z<360&&transform.eulerAngles.z>180){_correction=90f;}
        else if(transform.eulerAngles.z>0&&transform.eulerAngles.z<180){_correction=-90f;}
        else if(transform.eulerAngles.z==180){_correction=180f;}
        laserUpDirection = Quaternion.Euler(0f,0f,transform.eulerAngles.z+_correction) * Vector2.up;

        raycastDownOrigin = transform.position - (transform.up * (transform.localScale.y * 2f))+(transform.up*raycastLengthDown/3);
        raycastUpOrigin = transform.position-(transform.up*raycastLengthUp/2);//raycastLength gives it some buffer to detect when inside a wall
        
        LayerMask staticLayer=LayerMask.GetMask("StaticColliders");
        LayerMask laserLayer=LayerMask.GetMask("");
        if(otherLaserCollisionOn){
            laserLayer=LayerMask.GetMask("Lasers");
        }
        RaycastHit2D hitDown = Physics2D.Raycast(raycastDownOrigin, laserDownDirection, raycastLengthDown, staticLayer);
        RaycastHit2D hitDownLaser = Physics2D.Raycast(raycastDownOrigin, laserDownDirection, raycastLengthDown, laserLayer);
        RaycastHit2D hitUp = Physics2D.Raycast(raycastUpOrigin, laserUpDirection, raycastLengthUp, staticLayer);
        
        if((hitDown.collider!=null)||(hitDownLaser.collider!=null&&hitDownLaser.collider.gameObject!=gameObject/*&&hitDownLaser.collider.gameObject.GetComponent<Laser>().positive==!positive*/)){
            if(!touchingBottom){
                if(_posWhenTouched==Vector2.zero){_posWhenTouched=transform.position;}
                if(hitDown.collider!=null&&hitDown.collider.gameObject.CompareTag("Mirror")&&laserMirrored==null){ReflectLaserFromMirror(hitDown);}
                else if(hitDown.collider!=null&&!hitDown.collider.gameObject.CompareTag("Mirror")){AudioManager.instance.Play("Laser");}
                if(hitDownLaser.collider!=null&&hitDownLaser.collider.gameObject!=gameObject){AudioManager.instance.Play("LasersCollision");}
                touchingBottom=true;
            }
        }else if(hitDown.collider==null&&hitDownLaser.collider==false||(hitDownLaser.collider!=null&&hitDownLaser.collider.gameObject==gameObject)){
            touchingBottom=false;if(laserMirrored!=null){Destroy(laserMirrored.gameObject);laserMirrored=null;}mirrorReference=null;
        }

        if(hitUp.collider!=null){touchingUp=true;}
        else{touchingUp=false;}
    }
    ///Broken attempt at doing Mirror Reflection
    /*void ReflectLaserFromMirror(RaycastHit2D hit){
        if(laserMirrored == null && (laserParentReference == null || (laserParentReference != null && laserParentReference != this.gameObject && this.mirrorReference != laserParentReference.mirrorReference))){
            mirrorReference = hit.collider.gameObject;
            Vector2 mirrorNormal = hit.normal;
            Vector2 mirrorPosition = hit.transform.position;
            float mirrorRotation = hit.transform.eulerAngles.z;

            Vector2 reflectDir = Quaternion.Euler(0f, 0f, mirrorRotation) * Vector2.Reflect(laserDownDirection, mirrorNormal).normalized;
            float reflectionAngle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
            Debug.Log(laserDownDirection);
            Debug.Log(reflectionAngle);

            float mirrorFaceDirection = (mirrorRotation + 180f) % 360f;
            Debug.Log(mirrorFaceDirection);

            if(mirrorFaceDirection >= 0f && mirrorFaceDirection <= 180f){
                laserMirrored = Instantiate(this.gameObject, mirrorPosition, Quaternion.identity).GetComponent<Laser>(); // Clone
                laserMirrored.transform.eulerAngles = new Vector3(0f, 0f, reflectionAngle);
                laserMirrored.mirrorReference = hit.collider.gameObject;
                laserMirrored.laserParentReference = this.gameObject.GetComponent<Laser>();
            }
        }
    }*/
    /*void ReflectLaserFromMirror(RaycastHit2D hit)
        if(laserMirrored==null&&(laserParentReference==null||(laserParentReference!=null&&laserParentReference!=this.gameObject&&this.mirrorReference!=laserParentReference.mirrorReference))){
            mirrorReference=hit.collider.gameObject;

            Vector2 mirrorNormal=hit.normal;Vector2 mirrorPosition=hit.transform.position;
            float mirrorRotation=hit.transform.eulerAngles.z;float _correction=-90;
            float mirrorFaceDirection = (mirrorRotation + 180f) % 360f;//Calculate the mirror's face direction

            Vector2 reflectDir = Quaternion.Euler(0f,0f,mirrorRotation) * Vector2.Reflect(laserDownDirection, mirrorNormal).normalized;
            float reflectionAngle = (Mathf.Atan2(reflectDir.y,reflectDir.x) * Mathf.Rad2Deg);

            /*float laserAngle = Mathf.Atan2(laserUpDirection.y, laserUpDirection.x) * Mathf.Rad2Deg;
            if(mirrorFaceDirection>90f&&mirrorFaceDirection<270f){reflectionAngle+=180f;}//Facing downwards
            float minAngle = laserAngle - 90f;
            float maxAngle = laserAngle + 90f;
            float mirroredAngle = (reflectionAngle + 360f) % 360f;*/

            /*float laserAngle = Mathf.Atan2(transform.up.y, transform.up.x) * Mathf.Rad2Deg;
            float angleDifference = Mathf.DeltaAngle(laserAngle, reflectionAngle);*/

            /*float laserAngle = Mathf.Atan2(laserDownDirection.y, laserDownDirection.x) * Mathf.Rad2Deg;

            // Determine the correct reflection based on mirror face direction
            float correctedReflectionAngle = reflectionAngle + (mirrorFaceDirection - 180f);
            float angleDifference = Mathf.DeltaAngle(laserAngle, correctedReflectionAngle);

            if(reflectDir!=laserUpDirection&&(Mathf.Abs(angleDifference)<90f)){//Check if mirror is not pointing against, so the protected face etc
            //if(reflectDir!=laserUpDirection&&((mirrorFaceDirection>180f && angleDifference<0f)||(mirrorFaceDirection<180f&&angleDifference>0f))){//Check if mirror is not pointing against, so the protected face etc
            //if(reflectDir!=laserUpDirection&&(mirroredAngle>=minAngle&&mirroredAngle<=maxAngle)){//Check if mirror is not pointing against, so the protected face etc
                laserMirrored=Instantiate(this.gameObject,hit.transform.position,Quaternion.identity).GetComponent<Laser>();//Clone
                laserMirrored.transform.eulerAngles = new Vector3(0f, 0f, correctedReflectionAngle);
                laserMirrored.mirrorReference=hit.collider.gameObject;
                laserMirrored.laserParentReference=this.gameObject.GetComponent<Laser>();
            }
            /*Vector2 mirrorNormal=hit.normal;Vector2 mirrorPosition = hit.transform.position;
            float mirrorRotation=hit.transform.eulerAngles.z;float _correction=-90;
            float mirrorFaceDirection = (mirrorRotation + 180f) % 360f; // Calculate the mirror's face direction

            Vector2 reflectDir = Quaternion.Euler(0f,0f,mirrorRotation) * Vector2.Reflect(laserDownDirection, mirrorNormal).normalized;
            float reflectionAngle = (Mathf.Atan2(reflectDir.y,reflectDir.x) * Mathf.Rad2Deg)+_correction;
            if(mirrorFaceDirection>90f&&mirrorFaceDirection<270f){reflectionAngle+=180f;}//Facing downwards

            if(reflectDir!=laserUpDirection&&Vector2.Angle(reflectDir, mirrorNormal)<90f){//Check if mirror is not pointing against, so the protected face etc
                laserMirrored=Instantiate(this.gameObject,hit.transform.position,Quaternion.identity).GetComponent<Laser>();//Clone
                laserMirrored.transform.eulerAngles = new Vector3(0f, 0f, reflectionAngle);
                laserMirrored.mirrorReference=hit.collider.gameObject;
                laserMirrored.laserParentReference=this.gameObject.GetComponent<Laser>();
            }*/
            /*Vector2 reflectDir = Quaternion.Euler(0f, 0f, mirrorRotation) * Vector2.Reflect(laserDownDirection, mirrorNormal).normalized;
            
            laserMirrored = Instantiate(this.gameObject, mirrorPosition, Quaternion.identity).GetComponent<Laser>(); // Clone
            
            float incidentAngle = Mathf.Atan2(laserDownDirection.y, laserDownDirection.x) * Mathf.Rad2Deg;
            float reflectionAngle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
            
            Vector2 mirrorUpVector = Quaternion.Euler(0f, 0f, mirrorRotation) * Vector2.up;
            
            float dotProduct = Vector2.Dot(reflectDir, mirrorUpVector);
            float correctedAngle = reflectionAngle + 2f * (180f - incidentAngle) * dotProduct; // Calculate the corrected angle
            
            laserMirrored.transform.eulerAngles = new Vector3(0f, 0f, correctedAngle);
            laserMirrored.mirrorReference = hit.collider.gameObject;
            laserMirrored.laserParentReference = this.gameObject.GetComponent<Laser>();*/
        //}
    //}
    ///Only works for 0 degree colision so for going left
    // Vector2 mirrorFaceDirection;Vector2 mirrorPosition;
    // void ReflectLaserFromMirror(RaycastHit2D hit){
    //     Debug.Log("ReflectLaserFromMirror");
    //     if(laserMirrored==null&&(laserParentReference==null||(laserParentReference!=null&&laserParentReference!=this.gameObject&&this.mirrorReference!=laserParentReference.mirrorReference))){
    //         Debug.Log("Reflecting laser");
    //         mirrorReference=hit.collider.gameObject;
    //         Vector2 mirrorNormal=hit.normal;mirrorPosition=hit.transform.position;Vector2 hitPos=hit.point;
    //         float mirrorRotation=hit.transform.eulerAngles.z;float _correction=90f;
    //         mirrorFaceDirection = Quaternion.Euler(0f,0f,mirrorRotation+_correction) * Vector2.up;

    //         Vector2 reflectDir = mirrorFaceDirection * Vector2.Reflect(laserDownDirection, mirrorNormal).normalized;
    //         float reflectionAngle = Vector2.SignedAngle(mirrorFaceDirection, reflectDir);Debug.Log(reflectionAngle);//(Mathf.Atan2(reflectDir.y,reflectDir.x) * Mathf.Rad2Deg)+_correction;
    //         float dotProduct = Vector2.Dot(mirrorNormal, mirrorFaceDirection);Debug.Log(dotProduct);

    //         if(reflectDir!=laserUpDirection&&dotProduct>=0f){
    //             Debug.Log("Creating Reflected Laser");
    //             laserMirrored=Instantiate(this.gameObject,hitPos,Quaternion.identity).GetComponent<Laser>();//Clone
    //             laserMirrored.transform.eulerAngles = new Vector3(0f, 0f, reflectionAngle);
    //             laserMirrored.mirrorReference=hit.collider.gameObject;
    //             laserMirrored.laserParentReference=this.gameObject.GetComponent<Laser>();
    //             laserMirrored.clonedLaser=true;
    //             if(mirrorReference.GetComponent<Mirror>().opposite){laserMirrored.SetPolarity(!positive);AudioManager.instance.Play("ReflectLaserMirrorContrary");}
    //             else{AudioManager.instance.Play("ReflectLaserMirror");}
    //         }
    //     }
    // }
    ///Almost working attempt at Mirrors I think
    // void ReflectLaserFromMirror(RaycastHit2D hit){
    //     if(laserMirrored==null&&(laserParentReference==null||(laserParentReference!=null&&laserParentReference!=this.gameObject&&this.mirrorReference!=laserParentReference.mirrorReference))){
    //         Debug.Log("Reflecting laser");
    //         mirrorReference=hit.collider.gameObject;
    //         Vector2 mirrorNormal=hit.normal;
    //         float mirrorRotation=hit.transform.eulerAngles.z;float _correction=90;

    //         Vector2 reflectDir = Quaternion.Euler(0f,0f,mirrorRotation) * Vector2.Reflect(laserDownDirection, mirrorNormal).normalized;

    //         if(reflectDir!=laserUpDirection){
    //             Debug.Log("Creating Reflected Laser");
    //             laserMirrored=Instantiate(this.gameObject,hit.transform.position,Quaternion.identity).GetComponent<Laser>();//Clone
    //             float rotation = (Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg);// + _correction;
    //             laserMirrored.transform.eulerAngles = new Vector3(0f, 0f, rotation);
    //             laserMirrored.mirrorReference=hit.collider.gameObject;
    //             laserMirrored.laserParentReference=this.gameObject.GetComponent<Laser>();
    //             laserMirrored.clonedLaser=true;
    //             if(mirrorReference.GetComponent<Mirror>().opposite){laserMirrored.SetPolarity(!positive);AudioManager.instance.Play("ReflectLaserMirrorContrary");}
    //             else{AudioManager.instance.Play("ReflectLaserMirror");}
    //         }
    //     }
    // }
    ///New october solution test
    // void ReflectLaserFromMirror(RaycastHit2D hit){
    //     if (laserMirrored == null && (laserParentReference == null || (laserParentReference != null && laserParentReference != this.gameObject && this.mirrorReference != laserParentReference.mirrorReference))) {
    //         Debug.Log("Reflecting laser");
    //         mirrorReference = hit.collider.gameObject;

    //         // Calculate the mirror's rotation and apply an angle correction if necessary
    //         float mirrorRotation = hit.transform.eulerAngles.z;
    //         float angleCorrection = 0.0f; // Adjust this as needed
    //         float correctedRotation = (mirrorRotation + angleCorrection) % 360;

    //         // Get the surface normal of the mirror
    //         Vector2 mirrorNormal = Quaternion.Euler(0f, 0f, correctedRotation) * Vector2.up;

    //         // Calculate the reflection direction
    //         Vector2 reflectDir = Vector2.Reflect(laserDownDirection, mirrorNormal).normalized;

    //         // Check if the reflection direction is not the same as the laser's up direction
    //         if (reflectDir != laserUpDirection) {
    //             Debug.Log("Creating Reflected Laser");
    //             laserMirrored = Instantiate(this.gameObject, hit.transform.position, Quaternion.identity).GetComponent<Laser>(); // Clone

    //             // Calculate the rotation of the reflected laser
    //             float rotation = (Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg) + angleCorrection;
    //             laserMirrored.transform.eulerAngles = new Vector3(0f, 0f, rotation);
    //             laserMirrored.mirrorReference = hit.collider.gameObject;
    //             laserMirrored.laserParentReference = this.gameObject.GetComponent<Laser>();
    //             laserMirrored.clonedLaser = true;

    //             // Handle mirror polarity if necessary
    //             if (mirrorReference.GetComponent<Mirror>().opposite) {
    //                 laserMirrored.SetPolarity(!positive);
    //                 AudioManager.instance.Play("ReflectLaserMirrorContrary");
    //             } else {
    //                 AudioManager.instance.Play("ReflectLaserMirror");
    //             }
    //         }
    //     }
    // }
    void ReflectLaserFromMirror(RaycastHit2D hit) {
        if (laserMirrored == null && (laserParentReference == null || (laserParentReference != null && laserParentReference != this.gameObject && this.mirrorReference != laserParentReference.mirrorReference))) {
            Debug.Log("Reflecting laser");
            mirrorReference = hit.collider.gameObject;

            bool isNinetyDegree = mirrorReference.GetComponent<Mirror>().ninetydegree;

            // Calculate the mirror's rotation
            float mirrorRotation = hit.transform.eulerAngles.z;
            float angleCorrection = 0.0f;
            float correctedRotation = (mirrorRotation + angleCorrection) % 360;

            // Calculate the reflection direction based on the mirror type
            Vector2 reflectDir;
            if(isNinetyDegree){
                reflectDir = Quaternion.Euler(0f, 0f, correctedRotation + 90) * laserDownDirection;
            }else{
                reflectDir = Quaternion.Euler(0f, 0f, correctedRotation) * Vector2.Reflect(laserDownDirection, hit.normal).normalized;
                // reflectDir=Vector2.Reflect(laserDownDirection,hit.normal).normalized;
            }
            

            // Calculate the rotation of the reflected laser
            float rotation = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
            // Debug.Log(rotation);

            // Check if the reflection direction is not the same as the laser's up direction
            if(!isNinetyDegree && reflectDir!=laserUpDirection){
                Debug.Log("Creating Reflected Laser");
                laserMirrored = Instantiate(this.gameObject, hit.transform.position, Quaternion.identity).GetComponent<Laser>();

                laserMirrored.transform.localScale=new Vector2(laserMirrored.transform.localScale.x,0.1f);
                laserMirrored.transform.eulerAngles = new Vector3(0f, 0f, rotation);
                laserMirrored.mirrorReference = hit.collider.gameObject;
                laserMirrored.laserParentReference = this.gameObject.GetComponent<Laser>();
                laserMirrored.clonedLaser = true;
                // laserMirrored.otherLaserCollisionDelay = 0.1f;

                // Handle mirror polarity if necessary
                if(mirrorReference.GetComponent<Mirror>().opposite) {
                    laserMirrored.SetPolarity(!positive);
                    AudioManager.instance.Play("ReflectLaserMirrorContrary");
                }else{
                    AudioManager.instance.Play("ReflectLaserMirror");
                }
            }else if(isNinetyDegree&&Compute90DegreeReflectedAngle(this.transform.localEulerAngles.z,mirrorRotation)!=-1){
                laserMirrored = Instantiate(this.gameObject, hit.transform.position, Quaternion.identity).GetComponent<Laser>();

                rotation=Compute90DegreeReflectedAngle(this.transform.localEulerAngles.z,mirrorRotation);
                // Debug.Log(gameObject.name+" || "+this.transform.localEulerAngles.z+" | "+mirrorRotation+" = "+rotation);

                laserMirrored.transform.eulerAngles = new Vector3(0f, 0f, rotation);
                laserMirrored.mirrorReference = hit.collider.gameObject;
                laserMirrored.laserParentReference = this.gameObject.GetComponent<Laser>();
                laserMirrored.clonedLaser = true;
                // laserMirrored.otherLaserCollisionDelay = 0.1f;

                // Handle mirror polarity if necessary
                if(mirrorReference.GetComponent<Mirror>().opposite) {
                    laserMirrored.SetPolarity(!positive);
                    AudioManager.instance.Play("ReflectLaserMirrorContrary");
                }else{
                    AudioManager.instance.Play("ReflectLaserMirror");
                }
            }
            else{Debug.Log(gameObject.name+" || reflectDir is the same as the laser's: "+reflectDir);}
        }
    }
    Dictionary<(int, int), int> ninetyMirrorReflectionMap = new Dictionary<(int, int), int>
    {
        {(90, 90), 0},
        {(180, 180), 90},
        {(180, 90), 270},
        {(270, 180), 0},
        {(0, 270), 90},
        {(0, 0), 270},
        {(90, 0), 180},
        {(270, 270), 180}
    };
    float Compute90DegreeReflectedAngle(float entryAngle, float mirrorAngle){
        if(ninetyMirrorReflectionMap.TryGetValue(((int)entryAngle,(int)mirrorAngle),out int result)){return result;}
        Debug.Log("Incorrect angle on a 90degreemirror");
        return -1;
    }


    void OnDrawGizmos(){
        //#if UNITY_EDITOR
        /*Gizmos.color=Color.red;
        Gizmos.DrawRay(mirrorPosition,mirrorFaceDirection*4f);*/
        Gizmos.color=Color.green;
        Gizmos.DrawRay(raycastDownOrigin,laserDownDirection*raycastLengthDown);
        Gizmos.color=Color.blue;
        Gizmos.DrawRay(raycastUpOrigin,laserUpDirection*raycastLengthUp);
        //#endif
    }
    public void SwitchPolarity(){SetPolarity(!positive);return;}
    public void SetPolarity(bool _positive=true,bool force=true,bool quiet=false){
        if(spr==null){spr=GetComponent<SpriteRenderer>();}
        if(positive!=_positive||force){
            positive=_positive;
            if(positive){
                spr.sprite=spritencolor_positive.spr;
                if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=spritencolor_positive.color;
                if(!quiet){AudioManager.instance.Play("LaserChangePositive");AudioManager.instance.StopPlaying("LaserChangeNegative");}
            }else{
                spr.sprite=spritencolor_negative.spr;
                if(GetComponentInChildren<Light2D>()!=null)GetComponentInChildren<Light2D>().color=spritencolor_negative.color;
                if(!quiet){AudioManager.instance.Play("LaserChangeNegative");AudioManager.instance.StopPlaying("LaserChangePositive");}
            }
        }
    }
    void OnDestroy(){
        if(clonedLaser)LevelMapManager.instance.laserListSortedWithCloned.RemoveAll(x=>x==this);
    }
}
