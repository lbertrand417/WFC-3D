3D VERSION:

/!\ Data backup might not handled so when you reopen a scene it's better to restart from scratch

1) Create tiles
- Import/Create your prefab
- Add the script "Tile" to each one of your prefab
- Enter the "Offset" variable --> If your pivot point isn't at the center of your tile modify the offset so that it's placed at the center

2) Create the input example
- Create an empty object
- Add the script "SimpleTiledModelRules" to your object
- Choose the size of your canvas 
- Add the tile on the scene and put them as a CHILD of your input object
- Place them approximately and click on "RUN" to place them precisely 
/!\ Even if you place them precisely you must click on RUN (to actually store the tiles)
/!\ Each TYPE of tile should have the SAME name

You can clean (ie, remove tiles outside of the canvas) or clear (ie, remove all childs). Be sure to run before cleaning or clearing.

3) Generate the rules
After filling in the entire canvas, click on "Generate Rules"

4) Output 
- Create an empty object
- Add the script "WFC" to your object
- Select the width and height
- Select your input example and add it to your "Rules" variable
- Click on "RUN" to run the entire function, "RUN one step" to run only one step (= one propagation)

IMPORTANT: Make sure you didn't forget to generate rules in your input either way it won't work

You can restart the algorithm by clicking on "RESTART". 

IMPORTANT: We encourage you to use RESTART whenever you modify one of the variable (either the size or the input).



