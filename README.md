A small project recreating the maps that you would see in rogue likes similar to Slay the Spire in Unity utilizing Delaunay Triangulation and A* pathfinding.

![capture1](https://github.com/user-attachments/assets/cc67e357-b5d1-4954-ba22-655fe456eb17)
Picture of the Delaunay Trianglulation implementation

![capture2](https://github.com/user-attachments/assets/a8b8f9b6-18f4-48b9-9114-60ee09b2143b)
We pinpoint the start and the finish line (both denoted with a blue dot). The lowest point being out start point and the highest point being our goal

![capture3](https://github.com/user-attachments/assets/fa4f962d-e272-4d05-8658-5bacd775463e)
Using an A* pathfinding algorithm, we find a path to our goal

![capture4](https://github.com/user-attachments/assets/9a2206ad-7db2-4b9a-9dc7-36812cca39ef)
We run the A* pathfinding algorithm multiple times, each time *removing* a point that the map generator considers to add variety whilst also allowing for converging and diverging paths

![capture5](https://github.com/user-attachments/assets/4b692c1c-242b-450d-846d-ae76f68cd65e)
Finally we remove points that we don't need anymore, leaving us with our map!
