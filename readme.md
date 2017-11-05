PNG Pathfinding in C#

Dependencies:
EpPathFinding - by juhgiyo: https://github.com/juhgiyo/EpPathFinding.cs * 

*using C5 - by sestoft: https://github.com/sestoft/C5 

ImageProcessor - by JimBobSquarePants: https://github.com/JimBobSquarePants/ImageProcessor 


This project aims to find shortest paths using a simple PNG file as input. 

Currently it uses input.png as source file, it then does a series of image manipulations using the ImageProcessor NuGet

1. resizes the image
2. reduces the saturation as much as possible -100%
3. pixelates the image to fit the chosen grid granularity
4. turns the image blackwhite
5. export the image to output.png

Subsequently it loads the output.png as a bitmap which is fit on a grid using the EpPathFinding NuGet 
All dark points in the PNG are obstacles which the pathfinding needs to evade, white space is accessible
Finally pathfinding is executed from the EpPathFinding library, the settings can be easily adapted
A start and end location are initially set to the top left and bottom right corners
The resulting path is drawn in red on the image. If desired this can be exported as PNG or just displayed to the user