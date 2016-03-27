
    
function Update()
{
        if (Input.GetKey("1")) {
            GetComponent.<Animation>().CrossFade("run", 0.2);
        }

        else if (Input.GetKey("2")) {
            GetComponent.<Animation>().CrossFade("attack", 0.2);
        }

        else if (Input.GetKey("3")) {
            GetComponent.<Animation>().CrossFade("walk", 0.2);
        }

        else if (Input.GetKey("4")) {
            GetComponent.<Animation>().CrossFade("jump", 0.2);
        }

        else {
            if (!GetComponent.<Animation>().IsPlaying("jump"))
                GetComponent.<Animation>().CrossFade("idle", 0.2);
        }
    
}
    



    
    
    
    
    
