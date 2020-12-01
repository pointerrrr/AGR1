Besides the standard tasks we have also implemented the following:
•	Support for triangle meshes, using ‘obj’ files to import scenes
•	Texturing on all supported primitives, where the texture is a generic bitmap
•	Anti-aliasing
•	Added a sky dome
•	Multithreaded everything

Controls:
•	W,A,S, and D to move the camera
•	E and Q to move the camera up and down
•	Left and Right keys to rotate the camera left and right
•	Up and down keys to rotate the camera up and down
•	left bracket([) and right bracket(]) to increase and decrease the FOV

Work division: 50-50

References:
•	Sphere and skybox texturing is adapted from http://www.pauldebevec.com/Probes/
•	Random point on hemisphere at given normal adapted from https://www.gamedev.net/forums/topic/683176-finding-a-random-point-on-a-sphere-with-spread-and-direction/5315747/
•	Triangle intersection method found on https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
•	Triangle texturing from https://computergraphics.stackexchange.com/questions/1866/how-to-map-square-texture-to-triangle
•	Efficient barycentric coordinates for triangle texturing from  https://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates

Additional notes:
•   To switch from raytracing to path tracing (and vice versa) outcomment line 40 and uncomment line 41 in game.cs
•   To change the FOV, edit line 17 of GlobalLib.cs
•   To change the aspect ratio, edit line 18 of GlobalLib.cs (the Width and Height parameters are the dimensions of the screen in pixels, the aspect ratio will be calculated properly and the image will look natural)