
    
function Update()
{
        if (Input.GetKey("1")) {
            animation.CrossFade("run", 0.2);
        }

        else if (Input.GetKey("2")) {
            animation.CrossFade("attack", 0.2);
        }

        else if (Input.GetKey("3")) {
            animation.CrossFade("walk", 0.2);
        }

        else if (Input.GetKey("4")) {
            animation.CrossFade("jump", 0.2);
        }

        else {
            if (!animation.IsPlaying("jump"))
                animation.CrossFade("idle", 0.2);
        }
    
}
    



    
    
    
    
    
