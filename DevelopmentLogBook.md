# README #

This is the starter project for Rube Goldberg Challenge

### What is this repository for? ###

* You should find everything you need in `VRND_C8A_ProjectAssetPack/Assets`

## Project Notes
Looks like quite a lot of assets already supplied. Shouldn't be too hard to add in locomotion and stuff with it.

Mesh colliders are not very performant so we'd prefer to avoid those but for simple scenes they might be okay. 

Should we put a limit on how many objects you can spawn? 

I like the idea of having the controllers only pass input events (button presses, swipes, triggers, collisions) back up to the input manager, which decides what do with it. So yeah I'd like to use event delegates because thats a much neater pattern. Both controllers will have the same input detecting scripts on them, but no logic of what events they should cause. This makes our user input handling nice and modularly seperated from the actual input detection itself. 

Progress so far:
- Controller input manager sits at camera rig level
- Individual controllers pass input events (collisions, button / trigger presses) back up to input manager
- Point and click teleportation for left hand on ground plane
- Grip button head direction walk on ground plane with left hand also
- Right hand trigger grips objects, objects either, 'Throwable' - has controller velocity when released, 'Placeable' left in place (kinematic) when released
- Child colliders on platform, trampoline, metal plank provide complex shape colliders using primitives
- Child triggers on fan stream add force to objects in stream based on distance away
- Can pick up device objects from their child colliders by checking PlaceableChild tag and attaching parent instead to controller (had to set parent to not static)
- Since kinematic objects don't generate collisions with each other, had to give child colliders non-kinematic rigidbodies as well, with fixed X,Y,Z position and rotation so they stay fixed to their kinematic parents
- Put a blue cylinder under the pedestal in order to use it as a piston pump - cylinder should expand, push the head + ball with it

16/5/17
- Piston contraption animation
- Colliders
On my laptop, current speed of animation has the ball phase through, even with continuous dynamic set (probably no discrete graphics is taxing the CPU so we've got no physics time left...
Slower animations push the ball, but are much less interesting. 
Nothing to do with processor, had exactly the same issue on Desktop
Was also interested to note that setting the position programmatically, either using transform.position or the more recommended for kinematic objects, rigidbody.MovePosition(), didn't seem to affect the pedestal
There was one combination which seemed to move the ball, even though in the scene view neither the mesh nor the colliders moved. 
KINEMATIC OBJECT MOTION RESOLUTION:
- Make the kinematic object be a root level in the scene heirarchy, and have the colliders underneath it. When childed to other objects they really seemed to not cooperate, but when at root level everything started going smoothly - setting position using rigidbody.MovePosition moved the object very nicely. Now just need to test animation curves. 
- Have physics objects at top level of scene heirarchy
- Give the object a rigidbody, and either give it a collider, or give it empty game objects with colliders
- Do no scale the empty collider game objects, leave them at default, or at least scale them uniformly (all scales equal), and use collider size settings instead for performance
- Parent objects recieve triggers from their child object colliders
- THOUGHT: put the mesh renderer on its own game object so it can be scaled as needed without impacting the rest
- Nested game objects should really only be used for building up nice intricate static scenery from reusable assets, or for containing controller game objects, not for physics objects

So the piston is finally flipping working programmatically, and what a trek it has been. But man does it look super sexy now. So, don't trust physics and nest objects. 

I really like having the mesh on a seperate empty game object, so we can put it in with whatever scale it needs without needing to scale all the important physics and positioning stuff
Well ideally you'd design the mesh at the right size to start off with. The downside of this is that if you scale the whole object, your colliders won't scale properly (actually, yeah just scale it at top level, no problem)

Fixed trampoline using mesh collider in the shape of a cylinder - its not the most complex shape at least. 
Set it to convex expecting that to improve performance of it as well. 
Seems children pass trigger events back up to the parent as well as handling it themselves. 
- For the pedestal button, we let the parent handle the trigger because it is the only one
- For the fan stream trigger, each stream handles it and passes the event back up so the controller knows which streams are currently triggered. 

## 21/5/17
Object menu works great - touch the touchpad, the little icons show up. Press the top of the touchpad, make one appear, and release to place it (might need to adjust where each is placed by default so you don't end up with trampoline in the face)

Issue with picking up objects with child trigger colliders
- Grabbing the fan grabs the plank as well - even when the fan plank is a reasonable distance away. Why?
- Once the fan is childed to the right hand, the parent objects receive child triggers from all their children! 
- The fan jetstream is causing OnTriggerStay events to fire up the chain to the controller, which sees the plank as a placeable object, currently triggered by being in the fan jetstream, so it grabs hold. 
- Should we disable all colliders when we grab an object, and only re-enable when we release?
- Nope, by disabling them we lose knowing when to let them go! Haha We should probably push them into an array maybe instead of relying on triggers, then we release everythign we've grabbed on releasing the trigger whether or not something funny has happened. 
- We could always manually test distance and ignore if too far away - outside our hand sphere collider radius
- Nope with triggers we don't get collision points because they can fully intersect not just bounce. 
Solved by adding objects on grab onto a list of held objects, and releasing all held objects on trigger up. 
Also disabled all child colliders on grab, and re-enabled in the same release function described above

## 23/5/17
- Ball carried outside platform turns red
- Goes into Error layer
- Goal and collectibles go into objective layer

Added lots more colliders to environment, one robot arm, most boxes and the shelves, the machinary, some walls. 
This will allow us to design some more interesting levels with the platform and stuff in different areas. 

## 24/5/17
Collectible manager
Sounds for wood collisions, metal collisions, stone collisions, trampoline bounce, fan, teleport, piston woosh
Not bothering with wood, metal or stone collision differentiation, its only trampoline or other. Should be on the ball
What should manage everything? Level manager with logic to display text in front of user? (using their current forward direction flattened into the plane to determine where to put it. Needs to be screen overlay though in world space - can we do that? )
Welcome tutorial text:
"Welcome to the Contraption"
You have a number of items at your disposal to build a contraption which will collect all the stars. 

## 27/5/17
- Colliders on conveyer belts, tool stands, remaining boxes.
Done
- On star pickup play sound and release particles 
Done, also decrements count

- Make sure all static environment objects are marked static!
Done

- Ball resets on hitting ground plane - note that if you get it outside the walls, it will fall away into nothingness and not reset
- Add more wall colliders
Done

- Gentle background music - nosoapradio has nice selections, pick a few others to put in order and loop 
Done

- Ball & player start on platform, ball on pedestal, make sure cheat code is working properly

Do I put a different controller script on the weclome screen controller? I'd like them to be able to pick up the ball for starters. 

## 28/5/17
I don't like how I'm mixing my UI interaction code in here. How do I handle this better? 
Okay just need to make my functions public in input manager so that the buttons can select them as their on click event. Then I should be able to get a button reference from the raycast ui menu hit, and if a button is contained we highlight that button and store a reference to that putton. When we pull the trigger we call the click function on the button. If we don't hit a button we clear the highlithing on that button

- Welcome level contains 3D user interface and tutorial content. Trigger to continue (prefer pedestal with button which highlights on controller collision, but leave that). Put in its own level called "Welcome" and remove controller input handling to just triggers. Strong shading outside and bright light on start menu text
- Level manager shows level fail if goal reached without getting all collectibles. 
- Level manager shows level win otherwise. 
- Level transition loading with SteamVR
All done

## 6/6/17
- UI level fail Show collectibles collected. Note we need to override controller input manager on win / loose to advance level instead
- UI level win Show time taken + ball attempts made + percent contraptions used 
- Level design


## 1/7/17
- Sort out lighting design for other areas, particularly focusing on getting the right area for the level and fixing the flickering. Read up on new unity lighting system
Baking the lighting fixed the flickering issues

- Also need cheat code to catch when the ball is grabbed later outside the area and activate anti-cheat, at the moment it only works when 'carried' outside the area. Probably need a variable tracking whether we are inside the area or not, and set to disabled as soon as we grab it outside. Once grabbed outside the only way to play again is to reset the collectibles by throwing at ground - taking it back inside isn't good enough. You know, invalid ones should hit the collectibles so users know the path, but not hit the goal.
Any grab outside the start area sets the ball to error state until it is reset by hitting the ground

- Should also position win/loose screens in front of current head direction, not just in front of current fixed direction
Nah

- Is the roof upside down?
It was, flipped it

- Raycast up from ground hit to see if any other ground layer hit above. Means we will stand on top of the pedestal and on top of the shelves. (shelf collider might need to be slightly thinner). 
Nah never mind - you can just point at the shelf to go up there. 





Fixes:
- Trampoline bounce > 1? Repeated bounce went through roof then got stuck - sometimes it bounces > double height, maybe its hitting the inside mesh and combining the bounce?
- Performance improvements if required - less colliders? Compare against original scene
