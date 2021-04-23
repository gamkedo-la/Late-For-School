# Chunk Creation Steps

- Duplicate 'Chunk Template' in Assets/Prefabs/LevelDesign (Ctrl/Cmd+D)
- Rename duplicated prefab and move to 'Chunks' folder in Assets/Prefabs/LevelDesign
- Open duplicated prefab
- Drag in elements from the Platforms, Collectables, and Obstacles folders in Assets/Prefabs/LevelDesign
- Drag the 'ChunkStart' and 'ChunkEnd' game objects to the x positions where you want 
  the chunk to be loaded from and where you want the following chunk to be loaded from.
- In 'Main Scene' in Scene Hierarchy, click ChunkSpawner
- In inspector window:
	- Increment size of 'Chunks' array
	- Drag your newly created chunk prefab into the new slot

The chunk will now be randomly placed in the scene.
