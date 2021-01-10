# Chunk Creation Steps

- Duplicate 'Chunk Template' in Assets/Prefabs/LevelDesign
- Rename duplicated prefab and move to 'Chunks' folder in Assets/Prefabs/LevelDesign
- Open duplicated prefab
- Repeat the following until chunk is complete:
	- Drag in 'Platform prefab from Assets/Prefabs/LevelDesign
	- Resize platform as desired
- Drag the 'ChunkStart' and 'ChunkEnd' gameobjects to the x positions where you want 
the chunk to be loaded from and where you want the following chunk to be loaded from.
- In 'Main Scene' in Scene Hierarchy, click ChunkSpawner
- In inspector window:
	- Increment size of 'Chunks' array
	- Drag your newly created chunk prefab into the new slot

The chunk should be placed in the scene now randomly.
