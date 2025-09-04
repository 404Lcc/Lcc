## 5.3.8 (2025-06-13)
- Fixed an incompatibility with an older version of the unity collections package, which could cause an exception to be thrown when exiting the scene (introduced in 5.3.7).
- Fixed scanning very large layered grid graphs could throw an exception.
- Got rid of some small GC allocations in the example scenes relating to OnGUI calls.
- Fixed the update checker throwing exceptions in some rare cases. This was a regression in 5.3.5.

## 5.3.7 (2025-05-06)
- Significantly improved performance when scanning grid graphs when using Unity 6000.1+.
		This optimization has been tried multiple times before, but due to Unity bugs it has had to be rolled back.
		I think Unity has fixed the final physx bug relating to this now, so this optimization is back.
		Please report in the forum if you notice any hard crashes of the unity editor, when scanning grid graphs, after this update.
- Fixed using local avoidance on rotated graphs and isometric/hexagonal grid graphs could result in agents taking curved instead of straight paths.
- Fixed enabling thick raycasts on grid graphs did not do anything.

## 5.3.6 (2025-04-25)
- Fixed a tiny native memory leak accidentally introduced in 5.3.5.
- Fixed using the 'Duplicate Graph' button in the A* inspector would log a warning about duplicate guids.
- Reduced overhead of the RVO system when there are no agents using local avoidance in the scene.

## 5.3.5 (2025-04-22)
- Added \reflink{RecastGraph.collectionSettings.physicsScene} and \reflink{RecastGraph.collectionSettings.physicsScene2D} to allow specifying which physics scene to use when scanning a recast graph.
- Added \reflink{FollowerEntity.reachedCrowdedEndOfPath} which is like \reflink{FollowerEntity.reachedEndOfPath}, but will also return true if the end of the path is crowded, and this agent has stopped because it cannot get closer.
		\video{generated/scenes/Recast3D/crowdeddestination.webm}
- Fixed an edge case which could cause navmesh cutting to throw an exception (regression in 5.3.4).
- Fixed \reflink{FollowerEntity} would not take tags or penalties into account when simplifying its path on recast/navmesh graphs. This could cause it to move over high penalty areas that it should have avoided.
- Fixed \reflink{FollowerEntity} would behave strangely on isometric grid graphs.
- \reflink{FollowerEntity} now behaves better (though not perfectly) on hexagonal graphs.
- Fixed \reflink{FunnelModifier} would not take \reflink{ITraversalProvider}s into account when simplifying its path on recast/navmesh graphs.
- Fixed a minor GC allocation happening every frame when using URP.
- Fixed debug drawing would cause a minor overhead even in standalone builds, where it wasn't used.
- Fixed a significant memory leak when starting unity in batch mode.
- Fixed modifiers attached to a Seeker would not run when pathfinding is done outside of play-mode.
- When using \reflink{AstarData.DeserializeGraphsAdditive}, the new graphs will fill any empty slots in the array of graphs (if any graphs have been removed), instead of always being appended to the end of the array.
		This fixes the graphs array growing indefinitely when repeatedly removing and adding graphs in some cases.

## 5.3.4 (2025-03-18)
- Navmesh cuts now work much better in slopes.
		Previously, whenever the navmesh cut bounds touched a triangle bounds, the triangle would be cut by the full navmesh cut.
		Now, the full 3d extruded cut shape is used to calculate the cut. Resulting is more predictable cuts, and fixes
		a ton of edge cases where a navmesh cut could cause an agent to not be able to navigate across a seemingly navigable area.
		The video below shows the old behavior on the left, and the new one on the right.
		\video{changelog/navmeshcutfallingslowly_5_3_4_stack.webm}
- Navmesh cut gizmos are now oriented relative to the closest graph, instead of the first graph.
		This has no effect on the actual navmesh cut, but it makes it easier to see where the cut will be applied, in case there are multiple graphs in the scene.
- Fixed updating a recast graph on a unity terrain could in rare cases create reactangular gaps along a border of tiles.
- Reflection is now used to find graph types even in WebGL builds. This allows you to use custom graph types in WebGL builds too.
		Make sure your custom graph types are annotated with the [Pathfinding.Util.Preserve] attribute, otherwise they may be stripped out by the Unity WebGL build process.
- Fixed a bug causing navmesh cuts to add a tiny gap in the navmesh.

## 5.3.3 (2025-01-31)
- Fixed sometimes not being able to delete graphs from AstarPath components on prefabs.
- Fixed a tiny memory leak happening sometimes when editing a prefab with an AstarPath component.
- Fixed scanning grid graphs in Unity 6000.0.36f1+ would throw an exception, due to changes to unity's job system.
- Fixed setting Recast graph -> 'Filter Objects By' to Tags would incorrectly include everything in the scene.

## 5.3.2 (2025-01-27)
- Fixed compatibility with com.unity.entities version 1.3.9 (latest version at the time of this update).
- Fixed an out of range exception that could happen when using local avoidance and the latest burst package was installed (it was optimizing away my error checking!).
- Fixed a rare edge case that could cause the \reflink{FunnelModifier} on the High setting to produce a weird looking path that included some backtracking.
- Fixed an exception could be thrown when cameras were rendered without a color target.
- Fixed a race condition that could in rare cases cause an exception to be thrown from \reflink{FollowerEntity.SearchPath}.
- Fixed scanning grid graphs with invalid settings could throw an exception.
- \reflink{GridGraph.SetDimensions} will now throw an exception if the given width, depth or node size is less than or equal to zero.
- \reflink{MovementState.hierarchicalNodeIndex} and \reflink{MovementControl.hierarchicalNodeIndex} are now set to -1 at the end of the \reflink{AIMovementSystemGroup} to prevent accidental use of stale data.
- Fixed the \reflink{FollowerEntity} would sometimes still use gravity during traversal of off-mesh links, even if the built-in movement was disabled.
- Fixed a memory leak that could sometimes happen when calling \reflink{FollowerEntity.GetRemainingPath}.

## 5.3.1 (2025-01-13)
- Fixed a regression in 5.3.0 causing navmesh cutting to be much slower than it should be.
- Fixed triangle nodes on recast/navmesh graphs with no adjacent triangles would not be able to be traversed via off-mesh links.
- Fixed triangle nodes on recast/navmesh graphs with no adjacent triangles would not be detected by the \reflink{ConstantPath} path type.
- Fixed some missing videos in the \ref get-started-recast tutorial.

## 5.3.0 (2025-01-09)
- Added a new get started video:
		\youtube{PZXX4xGzCCA}
- Rewrote the \ref getstarted to make it easier to follow, and more up-to-date.
- Added a new tutorial: \ref get-started-grid.
- Added a new tutorial: \ref get-started-point.
- Added a new tutorial: \ref get-started-recast.
- Added a lightbox to all images in the documentation.
- Various other documentation improvements.
- Fixed enabling \reflink{FollowerEntity.isStopped} would make the agent resist being rotated by other means.
- Fixed \reflink{FollowerEntity} could vibrate a lot when being dragged outside the navmesh.
- The \reflink{FollowerEntity} control loop now always runs at least once per frame, instead of skipping some frames if the fps was very high.
- Fixed a bug causing paths calculating using the Manhattan or None heuristics on grid graphs to look much worse than they should (regression in 5.0).
		The paths were still optimal, but they were not as straight as they should have been.
- Fixed compilation errors when the "com.danielmansson.mathematics.fixedpoint" package was installed in the same project.
- Fixed some edge cases where navmesh cuts on recast graphs could break connections between adjacent tiles.
- The \reflink{FollowerEntity} component now defaults to drawing its path in the scene view. This can be disabled in the inspector, or by changing \reflink{FollowerEntity.debugFlags}.
- Fixed the \reflink{RuleTexture} on grid graphs would not necessarily realize if its texture reference had changed.
- The \reflink{RecastGraph} now defaults to a voxel size of 0.25, instead of 0.5.
		The default character radius was reduced a few versions ago, but the voxel size was not updated to match (it's recommended to keep the voxel size to at most half the character radius).
- Fixed the layer mask for the recast graph would be used even if the graph was set to filter by tags, not by layers.
- Fixed navmesh cuts could, in very degenerate cases, throw an exception due to an incorrect comparator.
- Fixed some edge cases where linecasts on recast/navmesh graphs could return that an obstacle existed if the end point of the linecast was exactly on the border between two nodes.
- Fixed an edge case when adding nodes to a point graph that could result in an exception being thrown.
- Local avoidance agents in layers that an agent does not try to avoid, no longer count towards the "Max Neighbours" limit.
- The FollowerEntity is will now use exponential back off if its path calculations continue to fail. Previously it would try to recalculate its path as quickly as possible in this case,
	   but now it will recalculate its path more slowly to improve performance.

## 5.2.5 (2024-11-20)
- Breaking changes
		- If you have built your own ECS baker for the FollowerEntity. You must now also add the \reflink{PhysicsSceneRef} component to the entity.
- When the \reflink{FollowerEntity} traverses off-mesh links and custom off mesh link handling code is used, it will no longer enable the agent's built-in movement by default.
		Instead it will only be enabled if \reflink{AgentOffMeshLinkTraversalContext.enableBuiltInMovement} is enabled or \reflink{AgentOffMeshLinkTraversalContext.MoveTowards} is called.
		This resolves issues many users have had where the agent's built-in movement was interefering with animation-driven movement during off-mesh links.
- Fixed navmesh cutting could throw an exception due to a multithreading race condition, if navmesh cutting was used and multiple regular graph updates were happening at the same time (regression in 5.2.0).
- Improved performance of \reflink{FollowerEntity} slightly.
- Fixed the inspector for \reflink{RulePerLayerModifications} would not properly show the tag dropdown (possibly only an issue in Unity 6).
- \reflink{RecastGraph.SnapBoundsToScene} will now make the bounding box taller to ensure the agent can stand on every valid surface.
- The \reflink{FollowerEntity} will now use the physics scene from its GameObject when performing raycasts. Previously it always used the default physics scene.
		This is useful when, for example, you use Unity's multiplayer testing mode, which creates multiple players in the same unity editor instance, each with their own physics scene.
- \reflink{RecastGraph}s now default to rasterizing colliders, instead of meshes. This has slightly better performance, and is a better choice for most games.
- Improved performance when deep profiling is enabled.
- Increased the minimum supported version of the (optional) entities package to 1.1.0 (up from 1.0.0).

## 5.2.4 (2024-10-14)
- Fixed trying to use navmesh cutting with an older version of the Unity collections package could throw an exception, instead of logging an error message saying that this is unsupported.
- Fixed an exception that could happen when setting FollowerEntity.movementPlaneSource to NavmeshNormal and the agent lost the path it was following.
- Fixed using the \reflink{NavmeshAdd} component could cause Unity to crash (regression in 5.2.0).
- Fixed incompatibility with the high definition render pipeline accidentally introduced in 5.2.3.
- Fixed the \reflink{FollowerEntity} component throwing an exception if it was opened in prefab isolation mode while the game was running.
- Fixed \reflink{NavmeshAdd.ForceUpdate} throwing an exception if there was no \reflink{AstarPath} component in the scene.
- Fixed gizmos were not rendered when opening prefab assets in isolation mode and the high definition render pipeline was used.
- Fixed a memory leak when the AstarPath component was put in a prefab.
- Fixed an exception that could happen if one tried to load a graph from a zip file that contained nodes (not just settings) into an AstarPath component on a prefab.

## 5.2.3 (2024-10-07)
- Fixed point graphs could in rare cases get stuck in an infinite loop when scanning.

## 5.2.2 (2024-10-05)
- \a Please \a delete \a your \a previous \a installation \a before \a upgrading.
- Fixed standalone builds would fail for some platforms and configuration options (regression in 5.2.0).
- Fixed changing the gravity setting on the \reflink{FollowerEntity} component during runtime would not affect anything.
- Changed \reflink{FollowerEntity.enableGravity} to have no effect if the agent's orientation is set to YAxisForward (2D mode).
		Gravity does not really make sense for top-down 2D games. The gravity setting is also hidden from the inspector in this mode.

## 5.2.1 (2024-10-03)
- Fixed upgrading to 5.2.0 from an older version could result in compilation errors. UnityPackages are truly terrible.
- Fixed a few edge cases with navmesh cutting (regression in 5.2.0).
- Improved navmesh simplification when using navmesh cutting.

## 5.2.0 (2024-10-01)
- Breaking changes
		- Renamed RecastMeshObj to \reflink{RecastNavmeshModifier}.
			An interface has been provided to handle backwards compatibility for most cases, but there are some cases where manual script changes are required (a search and replace will do it).
		- Renamed DynamicGridObstacle to \reflink{DynamicObstacle}, because it supports recast graphs too, since a while back.
			An interface has been provided to handle backwards compatibility for most cases, but there are some cases where manual script changes are required (a search and replace will do it).
		- The \reflink{NavmeshCut} component now requires Unity 2022.3 or newer to use, due to Unity bugs in earlier versions.
		- The \reflink{NavmeshCut} component now requires com.unity.collections version 2.2.0 or newer to use.
		- Moved \reflink{ListPool}, \reflink{ArrayPool} and \reflink{PathPool} to a new namespace Pathfinding.Pooling.
		- Removed all usages of the custom Int2 struct, and replaced them with Unity's equivalent Vector2Int struct.
- Improvements and new features
		- Significantly improved robustness of navmesh cutting.
			Previously, if navmesh cuts were placed in just the right way, the cutting could fail and the navmesh would not be updated correctly.
			This was rare, but if the game allowed user-placed navmesh cuts, it was bound to happen eventually.
		- Reduced GC allocations during navmesh cutting by up to 85%.
		- Improved performance of navmesh cutting by about 25%.
		- Navmesh cutting now works outside play mode too.
		- Added \reflink{AstarPath.Linecast}, which is a convenience method for checking if there is line-of-sight between two points.
			This will delegate the check to the most reasonable graph.
		- Added a tutorial on linecasting: \ref linecasting.
			\video{generated/scenes/Linecasting/linecast.webm}
		- Added a tutorial on how to add and remove nodes from a point graph: \ref creating-point-nodes.
		- Added \reflink{FollowerEntity.CreateEntity}. A static function to create an entity without using the FollowerEntity MonoBehaviour itself.
		- Added \reflink{NavGraph.RandomPointOnSurface} to get a random point on the surface of a graph, optionally filtered by a \reflink{NNConstraint}.
		- Nodes in point graphs can now be removed using \reflink{PointGraph.RemoveNode}. Previously it was only possible to add nodes.
		- Point graphs will now draw a box around each node in the scene view, even for nodes that have been added using a script.
		- It's now possible to duplicate graphs in the inspector.
		- Improved styling of graph edit icons in the inspector:
			\shadowimage{changelog/edit_icon2.png}
		- Added \reflink{AstarData.DuplicateGraph}.
		- Updated various screenshots in the documentation to be more up to date, and in HiDPI, so they look great on retina displays too.
		- \reflink{FollowerEntity.velocity} can now be assigned to.
		- Added \reflink{IAstarAI.updatePosition} and \reflink{IAstarAI.updateRotation}. These properties have existed on all movement scripts, but they were not added to the interface before.
		- Reduced GC allocations and improved performance slightly, when loading a sequence of scenes that contain the \reflink{AstarPath} component.
- Fixes
		- Fixed \reflink{ManagedState.Clone} would throw an exception if its PathTracer was not initialized.
		- Fixed \reflink{SingleNodeBlocker.Block} would not unblock the previously blocked node (in contrast to what the documentation said, and different from what the BlockAt method did).
		- Fixed layered grid graphs could in rare cases generate one-way connections between nodes, when \reflink{GridGraph.maxStepUsesSlope} was enabled.
		- Fixed the graph display window in the scene view would be rendered off-screen on HiDPI displays.
		- Fixed the \reflink{GraphUpdateScene} shortcuts help window in the scene view would be rendered off-screen on HiDPI displays.
		- Fixed \reflink{FollowerEntity.SearchPath} would not take into account any custom pathfinding settings set on the \reflink{FollowerEntity} component.
		- Fixed undo events could in some situations reset pathfinding settings on the \reflink{FollowerEntity} component.
		- Fixed calling \reflink{FollowerEntity.GetRemainingPath} could throw an exception if the path contained off-mesh links (recent regression).
		- Fixed having \reflink{FollowerEntity.isStopped} enabled when a FollowerEntity component was enabled, could cause its rotation to be reset.
		- Fixed the \reflink{FollowerEntity} could attach itself to a graph that did not match its graph mask, for a few frames, until its first path was calculated.
		- Fixed updating a layered grid graph and forcing nodes to be walkable, could, in some cases, create connections to nodes that didn't exist, causing errors later on.
		- Reduced "shyness" around locked local avoidance agents close to the end of an agent's path.
		- Fixed the heuristic optimization subsystem could throw an exception in some cases if all graphs were empty.
		- Fixed scanning a recast graph with very tiny \reflink{RecastNavmeshModifier} components could in exceedingly rare cases freeze Unity.
		- Fixed the \ref writing-graph-generators tutorial created node connections incorrectly (regression in 5.0).
		- Fixed one overload of NavmeshBase.Linecast would ignore the \a hint parameter.
		- Worked around a Unity bug which could cause the welcome screen to throw an exception.
- Changes
		- Removed obsolete script MineBotAI. It has been deprecated for around 7 years and has been replaced by AIPath together with MineBotAnimation.
		- Removed the experimental warning for the \reflink{FollowerEntity} component.
		- \reflink{RecastGraph.characterRadius} now defaults to 0.5, instead of 1.5. This is a more reasonable default for most humanoid characters.
		- Split out the \reflink{RepairPathSystem} from the \reflink{FollowerControlSystem}.
		- Added the DisallowMultipleComponent attribute to the AstarPath component, and all built-in movement scripts.
		- Renamed AstarData.navmesh to \reflink{AstarData.navmeshGraph} for consistency.
		- \reflink{NavGraph.RelocateNodes} will now verify that it is safe to update nodes. It's safe to update nodes in a work item, or during a graph update.
		- Moved various internal classes to a new namespace Pathfinding.Collections.
		- Moved various internal classes to a new namespace Pathfinding.Sync.
		- Moved \reflink{ListPool}, \reflink{ArrayPool} and \reflink{PathPool} to a new namespace Pathfinding.Pooling.
		- Removed AIBase.centerOffset, as it's been deprecated for 6 years.
		- Removed AIBase.rotationIn2D, as it's been deprecated for 6 years. Use \reflink{AIBase.orientation} instead.
		- Removed AILerp.rotationIn2D, as it's been deprecated for 6 years. Use \reflink{AILerp.orientation} instead.
		- Removed AIBase.target, as it's been deprecated for 7 years. Use the \reflink{AIDestinationSetter} component instead.
		- Removed AILerp.target, as it's been deprecated for 7 years. Use the \reflink{AIDestinationSetter} component instead.
		- Removed AILerp.ForceSearchPath, as it's been deprecated for 7 years. Use \reflink{AILerp.SearchPath} instead.
		- Removed GridNode.GetConnectionInternal, as it's been deprecated for 8 years. Use \reflink{GridNode.HasConnectionInDirection} instead.
		- Removed LevelGridNode.HasConnection, as it's been deprecated for 8 years. Use \reflink{LevelGridNode.HasConnectionInDirection} instead.
		- Removed RVOController.mask, as it's been deprecated for 7 years. Use settings on your movement script instead.
		- Removed RVOController.enableRotation, as it's been deprecated for 7 years. Use settings on your movement script instead.
		- Removed RVOController.rotationSpeed, as it's been deprecated for 7 years. Use settings on your movement script instead.
		- Removed RVOController.maxSpeed, as it's been deprecated for 7 years. Use settings on your movement script instead.
		- Removed RVOController.ForceSetVelocity, as it's been deprecated for 7 years. Use \reflink{RVOController.velocity} instead.
		- Removed RVOController.Teleport, as it's been deprecated for 8 years. It is no longer necessary.

## 5.1.6 (2024-08-06)
- Fixed compatibility with com.unity.mathematics 1.3.0 and lower.

## 5.1.5 (2024-08-06)
- If an async scan is running when the AstarPath component is disabled, or another scan is started, then the code will block until the in-progress async scan has finished.
		Previously, unloading a scene while an async scan was running could in some cases lead to memory leaks and exceptions.
- Fixed \reflink{FollowerEntity} could get teleported a small distance in 2D games, right when the game started.
- Fixed \reflink{PointGraph.GetNearest} could throw an exception in some situations if there was no acceptable node nearby.
- Fixed \reflink{GridGraph.RelocateNodes} could leave the grid graph with a corrupted state.
- Fixed updating point graphs could throw an exception if some nodes had been manually added to the graph.
- Fixed some compilation warnings in Unity 6.
- Fixed \reflink{FollowerEntity.updateRotation} and \reflink{FollowerEntity.updatePosition} would not always take effect when set from the unity inspector.
- Fixed \reflink{AstarPath.showSearchTree} could show incorrect information on recast graphs.
- Fixed \reflink{FloodPathTracer} not working with recast graphs (regression in 5.0).
- Fixed using graph coloring modes G, H or F on recast graphs could display incorrect information.
- Fixed toggling \reflink{AstarPath.showSearchTree} would not always re-render the graph visualization.
- Fixed an \reflink{ITraversalProvider} on a \reflink{FollowerEntity} could be called with a null node in some cases.
- Fixed tons of grammar and spelling mistakes in the documentation.
- Fixed Seeker.StartMultiTargetPath would never use the Seeker's graphMask.
- Fixed calling \reflink{AstarPath.FlushWorkItems} would freeze unity if a work item threw an exception.
- \reflink{AstarData.DeserializeGraphsAdditive} now returns an array with the deserialized graphs.
- Trying to deserialize graphs from an invalid zip file will now throw an exception instead of just logging an error message.
- Renamed RecastGraph.SnapForceBoundsToScene to \reflink{RecastGraph.SnapBoundsToScene}.
- RecastGraph.SnapBoundsToScene will now ensure the resulting bounds are non-zero along all axes.
- Improved smoothness of the \reflink{FollowerEntity} when using the \reflink{MovementPlaneSource}.NavmeshNormal mode.
- The \ref spherical example scene now uses the \reflink{FollowerEntity} component instead of the \reflink{AIPathAlignedToSurface} component.
- \reflink{AstarPath.showSearchTree} now renders the search tree with thicker lines, to make it easier to see.
- Deprecated \reflink{PointNode.SetPosition}. Use \reflink{PointNode.position} instead.
- Fixed \reflink{GridGraphRule.Pass}.AfterApplied not being called in some cases.

## 5.1.4 (2024-06-12)
- Fixed an exception that could be thrown after updating a graph (regression in 5.1.3).
- Reduced memory usage slightly.
- Improved performance when updating a recast graph in a scene with a terrain.

## 5.1.3 (2024-06-11)
- \reflink{FollowerEntity.updatePosition} and \reflink{FollowerEntity.updateRotation} can now be set in the inspector, and the values are no longer reset if the component is disabled.
- Fixed Unity's Random state would get reset by the FollowerEntity movement script on scene initialization.
- Paths now take their assigned \reflink{ITraversalProvider} into account when finding the start and end nodes.
		This fixes several issues where an agent could get stuck and not be able to calculate a path, because the node it was standing on (or the node closest to the destination)
		was walkable according to the path's \reflink{NNConstraint}, but not according to its \reflink{ITraversalProvider}.
- Fixed \reflink{FollowerEntity} sometimes getting stuck when traversing non-flat grid graphs.
- Fixed \reflink{FollowerEntity} could throw an exception if it was disabled while it was traversing an off-mesh link.
- Exposed a few more properties in the \reflink{FollowerEntity} class, and improved its documentation.
- The number of temporary nodes for path calculations can now grow automatically. Therefore the ASTAR_MORE_MULTI_TARGET_PATH_TARGETS define is no longer needed.
		This was only of importance if you used multi target paths with a lot of targets, or if you had a very large number of off-mesh links in your scene.
- Fixed the 'Round Collider Detail' setting was not visible in the recast graph inspector when in 2D mode in some cases, even when it should.
- Fixed \reflink{AstarData.DeserializeGraphsAdditive} would not remove null graphs from the end of the list of graphs before appending the new ones.
- Fixed disabling a \reflink{NavmeshPrefab} component could later cause a null reference exception when the graph was destroyed.
- Fixed \reflink{NavmeshPrefab}s could sometimes leave a 1 voxel wide gap at their borders (recent regression).
- Various documentation improvements.

## 5.1.2 (2024-05-29)
- Added \reflink{AstarPath.IsPointOnNavmesh} to more easily check if a point is on the navmesh surface of any graph.
- Added \reflink{NavGraph.IsPointOnNavmesh}, which works the same, but only checks a single graph.
- Fixed \reflink{ProceduralGraphMover} would not scan new tiles correctly when moving in the +x or +z directions (recent regression).
- Fixed the recast graph inspector would not show the Character Radius field in 2D mode (regression in 5.1.0).

## 5.1.1 (2024-05-08)
- Added a welcome screen when importing the package into a new project.
		It makes it easy to import the example scenes into the project, something new users are often confused about how to do.
		It also has convenient links to the documentation and changelog.
		\shadowimage{changelog/welcome_screen.png}
- Added \reflink{RecastGraph.collectionSettings.onCollectMeshes} to allow adding custom meshes to be rasterized by the recast graph.
- Fixed \reflink{FollowerEntity} could throw an exception when it was scaled by a negative value.
- Improved compatibility with other packages. Previously, \reflink{FollowerEntity} could throw an exception in some projects if, for example, DoTween Pro was installed.
- Improved styling of the documentation.

## 5.1.0 (2024-05-02)
- Breaking changes
		- It is no longer possible to use both the layer mask and tag mask at the same time when scanning recast graphs.
			The tag mask was very seldom used and it was a bit confusing to have both options available at the same time in the inspector.
- New Features and improvements
		- Tags applied when recast graphs are scanned will now be preserved even when using navmesh cutting.
			Previously, navmesh cutting would not be able to preserve these tags.
			Note that this only applies to tags that are set either on a \reflink{RecastNavmeshModifier} component, or using a graph update that happens during the initial scan.
		- Navmesh cutting is now better at collapsing adjacent triangles into fewer triangles, if possible.
			This can improve pathfinding quality, and improve performance ever so slightly.
		- Restructured the recast graph inspector, with clearer headings and a more consistent layout.
			\shadowimage{changelog/recastgraph_inspector.png}
		- Recast graphs now have a new option for per layer modifications.
			This means you can now set tags or make nodes unwalkable based on the layer of the surface under the node, without having to attach a \reflink{RecastNavmeshModifier} component to every object.
			\shadowimage{recast/per_layer_modifications.png}
		- \reflink{MecanimBridge} now supports the \reflink{FollowerEntity} movement script.
		- There's now a button in all relevant example scenes that link them to the corresponding page in the documentation.
		- When the scene view camera is in 2D mode, one can now add points to the \reflink{GraphUpdateScene} by shift-clicking even in empty areas of the screen. Previously, one had to click on a GameObject.
			This should improve the workflow when working on 2D games.
		- Implemented \reflink{FollowerEntity.Move}.
		- Changing \reflink{FollowerEntity.enableGravity} doesn't cause a structural entity change anymore, which means it can be used from an off-mesh link handler.
- Fixes and changes
		- Fixed scanning recast graphs asynchronously could block due to it creating too many jobs, so that Unity forced some to execute on the main thread.
		- Fixed a few edge cases when scanning recast graphs.
		- Fixed asynchronously scanning grid graphs would not split up some main thread work into smaller chunks, which could cause frame drops.
		- Fixed \reflink{FollowerEntity} could throw exceptions when traversing a grid graph that was being updated at the same time.
		- Fixed when adding the first point to a \reflink{GraphUpdateScene} component by shift-clicking in the scene view, the GameObject could end up being deselected.
		- Recast graphs now handle degenerate triangles a bit better.
		- Fixed tiled recast graphs could generate a navmesh outside the bounding box of the graph, if the tile size didn't divide the graph size evenly.
		- Fixed \reflink{FollowerEntity} could get stuck when one of its world coordinates were around 2100, due to coordinate overflows in some calculations.
		- Added a warning in the grid graph inspector when trying to use the \reflink{RulePerLayerModifications;per layer modifications rule} without height testing enabled.
		- Minor other improvements to editing points for the \reflink{GraphUpdateScene} component.
		- Renamed RecastGraph.rasterizeColliders to \reflink{RecastGraph.collectionSettings.rasterizeColliders}.
		- Renamed RecastGraph.rasterizeMeshes to \reflink{RecastGraph.collectionSettings.rasterizeMeshes}.
		- Renamed RecastGraph.rasterizeTerrain to \reflink{RecastGraph.collectionSettings.rasterizeTerrain}.
		- Renamed RecastGraph.rasterizeTrees to \reflink{RecastGraph.collectionSettings.rasterizeTrees}.
		- Renamed RecastGraph.colliderRasterizeDetail to \reflink{RecastGraph.collectionSettings.colliderRasterizeDetail}.
		- Renamed RecastGraph.mask to \reflink{RecastGraph.collectionSettings.layerMask}.
		- Renamed RecastGraph.tagMask to \reflink{RecastGraph.collectionSettings.tagMask}.
		- Renamed RecastGraph.terrainSampleSize to \reflink{RecastGraph.collectionSettings.terrainHeightmapDownsamplingFactor}.

## 5.0.9 (2024-04-15)
- Minor internal changes.
- Various minor documentation improvements.
- Reduced allocations during pathfinding slightly.

## 5.0.8 (2024-04-12)
- Made \reflink{AstarPath.PausePathfindingSoon} public.
- Made \reflink{RepairPathSystem.ResolveOffMeshLinkHandler} public.
- Made \reflink{RepairPathSystem.NextLinkToTraverse} public.
- Added a public constructor for \reflink{MovementTarget}.

## 5.0.7 (2024-04-11)
- New Features and improvements
		- Improved the \ref calling-pathfinding documentation page.
		- Improved support for \reflink{FollowerEntity} on spherical (or other weirdly shaped) worlds, using the new \reflink{MovementPlaneSource}.NavmeshNormal mode.
			\video{generated/scenes/WonkyNavmesh/overviewvideo.webm}
		- Explicitly reference .dll files in the project to improve compatibility with some IDEs.
		- Added \reflink{AstarPath.GetNavmeshBorderData}.
		- Added \reflink{AgentOffMeshLinkTraversalContext.Abort}, which can be used to abort traversal of an off-mesh link, from the coroutine that handles it.
		- Improved performance of the \reflink{FollowerEntity} component.
- Fixes and changes
		- The \reflink{AstarPath} component now shows up as "AstarPath" in the component menu, instead of "Pathfinder". This should make it easier to find for new users, as it is referred to as AstarPath in the documentation.
		- Fixed the agent would get stuck on obstacles placed using the "P" key in the "InfiniteWorld" example scene, due to incorrect layer settings.
		- Fixed various edge cases when the AstarPath component is put in a prefab.
		- Fixed \reflink{FollowerEntity.GetRemainingPath} throwing an exception in some cases (regression in 5.0.6).
		- Fixed \reflink{AstarPath} drawing graphs even when an unrelated prefab was opened in isolation mode.
		- Fixed division by zero in \reflink{FollowerEntity} when rotation speed was zero.
		- Fixed a race condition when scanning or updating recast graphs, which could cause graph updates to throw exception in very rare cases.
		- Fixed \reflink{RecastNavmeshModifier} with the WalkableSurfaceWithTag mode could fail to apply the tag if it was also marked as solid, and the surface was close to another surface.
		- Fixed a small GC allocation happening every frame when using the \reflink{FollowerEntity} component.
		- Fixed the gravity applied to the \reflink{AIPathAlignedToSurface} movement script was much higher than it should be.
		- Fixed the \reflink{AIPathAlignedToSurface} movement script could throw an exception when completely inside a convex mesh collider.
		- Fixed \reflink{NodeLink2} components could throw an exception if they were loaded in as enabled, but then immediately destroyed before Unity had a chance to call the OnEnable method.
		- Reduced max graph count from 256 to 255.
		- Added \reflink{GraphNode.InvalidGraphIndex}.
		- Renamed AgentOffMeshLinkTraversalContext.linkInfo to \reflink{AgentOffMeshLinkTraversalContext.link}.

## 5.0.6 (2024-03-29)
- Added \reflink{IAstarAI.GetRemainingPath(List<Vector3>,List<PathPartWithLinkInfo>,bool)} to get information about all the parts (including off-mesh links) of the path that the agent is currently following.
		\shadowimage{generated/scenes/RecastOffMeshLinks/remainingpath.png}
- Fixed \reflink{FollowerEntity.GetRemainingPath(List<Vector3>,bool)} would only output the path up to the next off-mesh link.
- Fixed \reflink{FollowerEntity} could return incorrect values for a few properties (e.g. \reflink{FollowerEntity.hasPath}) after it had been disabled.
- Fixed \reflink{RandomPath} could in very rare situations cause an exception that crashed the pathfinding threads, due to a race condition.
- Fixed \reflink{MultiTargetPath} could calculate incorrect paths on grid graphs in some situations.
- Fixed various edge cases when the AstarPath component is put in a prefab.
- Fixed \reflink{RecastNavmeshModifier} components could log an error message about being moved, even if they had not moved.
- Fixed enabling \reflink{ABPath.calculatePartial} could cause an exception or an incorrect path to be calculated, in some situations.
- Fixed several broken documentation links.
- If an \reflink{RVOSimulator} is enabled in a scene that already has an active RVOSimulator, the new one will be disabled and a warning will be logged.
		This now works like the \reflink{AstarPath} component, which also uses a singleton pattern.

## 5.0.5 (2024-03-21)
- Added \reflink{FollowerEntity.movementOverrides} to allow more easily overriding custom movement logic for the \reflink{FollowerEntity} component.
		\video{generated/scenes/RecastTerrain/movementoverride.webm}
- Made \reflink{GraphUpdateScene.GetGraphUpdate} virtual.
- Fixed agents with the \reflink{FollowerEntity} component trying to traverse an off-mesh link could get stuck waiting for a long time at the beginning of the link, if either local avoidance or rotation smoothing was enabled.
- Fixed help links for components used an incorrect extension, and would therefore often not work.

## 5.0.4 (2024-03-15)
- Added \reflink{RVOController.SetObstacleQuery}.
- Added \reflink{GridGraph.GetBoundsFromRect}.
- Exposed \reflink{AstarPath.Snapshot}.
- Improve the \ref upgrading.
- Clarified the \ref installation.
- Fixed \reflink{NavmeshAdd} components not affecting completely empty recast graph tiles.
- Fixed \reflink{RichAI.steeringTarget} being incorrect when very close to the destination.
- Fixed \reflink{NavmeshBase.PointOnNavmesh} could return null right on the edge beteween two triangles, in some cases.
- Fixed an exception that could happen when a degenerate triangle was generated when scanning a recast graph or a navmesh prefab.
- Fixed exception that could happen sometimes when the \reflink{FollowerEntity} was simplifying its path on a grid graph.
- Fixed \reflink{GridGraph.SetWalkability} not refreshing off-mesh links.

## 5.0.3 (2024-03-09)
- Added \reflink{NavmeshPrefab.removeTilesWhenDisabled}.
- The \reflink{NavmeshPrefab} will now add its stored tiles to the graph every time the component is enabled, instead of only the first time.
- Clarified the \ref installation.
- Fixed teleport link in the off-mesh link example scene only worked in the start->end direction, but not in the end->start direction.
- Fixed \reflink{FollowerEntity} not taking penalties into account when simplifying its path on grid graphs.
- Fixed some data migrations for components would not run.
- Fixed compilation errors in Unity 2022.1.
- Fixed compilation warnings when using older versions of the collections package (before 2.1.0).
- Fixed \reflink{RVOSimulator.useNavmeshAsObstacle} not working with the \reflink{AIPath} and \reflink{RichAI} movement scripts.
- Renamed AgentOffMeshLinkTraversal.firstPosition to \reflink{AgentOffMeshLinkTraversal.relativeStart}.
- Renamed AgentOffMeshLinkTraversal.secondPosition to \reflink{AgentOffMeshLinkTraversal.relativeEnd}.

## 5.0.2 (2024-03-06)
- Fixed some example scenes contained objects only intended for development, with missing scripts.

## 5.0.1 (2024-03-05)
- The Asset Store version of the package is now distributed as an NPM package, instead of a UnityPackage.
		This should make it easier to install and update the package.

## 5.0 (2024-03-05)
- <b>Read more about the new features in the <a href="https://arongranberg.com/2024/02/a-pathfinding-project-5-0/">blog post</a></b>.
- These release notes combine all changes that were evaluated in the 4.3.x beta versions which lead up to the 5.0 release.
- <b>Major New Features</b>
		- Burst powered scanning of recast graphs.
			Recast graphs now use the Burst Compiler and the Unity Job System.
			Together with a lot of work on improving the underlying algorithms, this has resulted in much faster scans and updates of recast graphs.
			It’s now up to 3.5x faster. Graph updates and async scans also run almost entirely in separate threads now, to make the fps impact as minimal as possible.
		- Burst powered scanning of grid graphs and layered grid graphs.
			Grid graphs have received a major rewrite in how they scan the graph.
			It is now powered by burst and the unity job system, which makes scanning graphs around 3x faster.
			It also supports new features like the \reflink{GridGraph.maxStepUsesSlope;Max Step Uses Slope} option,
			which makes the graph much better connected in most scenarios involving slopes.
				When upgrading this field will be disabled for compatibility reasons, but it will be enabled for all new graphs.
				\shadowimage{gridgraph/max_step_uses_slope.png}
			- There's also a whole new system for creating rules for grid graphs.
			These rules allow you to tweak the grid graph scanning process in a very flexible way, which also works with graph updates
			transparently. See \ref grid-rules.
			- For 2D enthusiasts, the grid graph can now align itself to a Tilemap directly from the inspector.
				Something which otherwise might require complex trigonometry, especially for isometric games, or those using hexagonal tiles.
				\shadowimage{gridgraph/tilemap_result.png}
			- More accurate and faster linecasts on grid graphs
			Linecasts on grid graphs have received a major rewrite, with a big focus on robustness and correctness.
			Previously, edge cases like "the linecast goes precisely along the border to an unwalkable obstacle", or "the linecast ends exactly at a corner of an obstacle",
			were not well defined. Now, the between the walkable part of the navmesh and the obstacles is defined as walkable.
			Edge cases are handled a lot better now.
		- New local avoidance algorithm
			- The new local avoidance algorithm based on ORCA (Optimal Reciprocal Collision Avoidance), leads to nicer movement in most cases.
			- It's also faster! Not only due to the new algorithm, but also because now everything is powered by burst and the unity job system.
				Local avoidance is up to <b>10x faster</b> than in 4.2.x.
			- Takes the navmesh into account.
				The local avoidance system now takes the navmesh into account automatically, and with good performance.
				Previously, the RVONavmesh component has been used for a similar result, but it had much worse performance,
				and it was not as robust.
			\shadowimage{rvo/rvo_stress_30000_white.png}
		- New ECS-powered movement script: \reflink{FollowerEntity}.
			The focus for this new movement script is that it should “just work”, and that it should be very robust and handle all those pesky edge cases.
			In terms of performance, it’s also slightly better than both the AIPath and RichAI movement scripts.
			Out of the box, it supports local avoidance, smooth and very responsive movement, off-mesh links, staying away from walls, facing a given direction when it reaches its destination, and a lot more.
			You do not need to know anything about ECS to use it, everything is abstracted away, and you can use it much like the other movement scripts.
		- The \reflink{ProceduralGraphMover} (previously ProceduralGridMover) now supports recast graphs too.
			Now your infinite worlds can have recast graphs too!
			\video{recast/graph_mover.webm}
		- Improved off-mesh links
			Off-mesh links have been almost completely rewritten to be more robust and easier to use.
			You can now specify a tag for them, and choose which graphs they are allowed to link.
			They are also much more robust when combined with graph updates, whereas previously they could sometimes lose their connection to the navmesh.
			And during those graph updates, they require much less cpu power to refresh themselves, so now you can have a lot more of them in your scene without any performance issues.
			Take a look at the new tutorial on off-mesh links: \ref example_recast_offmeshlinks.
			\video{generated/scenes/RecastOffMeshLinks/interact.webm}
		- Added support for all 'Enter Play Mode Options' options.
			This means you can cut down on your iteration times significantly. See https://docs.unity3d.com/Manual/ConfigurableEnterPlayMode.html
		- Improved Documentation
			- Added new tutorials and documentation pages:
				- \ref offmeshlinks.
					\video{generated/scenes/RecastOffMeshLinks/interact.webm}
				- \ref tilemaps.
					\shadowimage{gridgraph/tilemap_result.png}
				- \ref move_in_circle.
					\video{move_in_circle/move_in_circle.mp4}
				- \ref moving_graph.
				- \ref traversal_provider.
				- \ref grid-rules.
				- \ref grid-rules-write.
				- \ref playermovement.
			- Added 6 new example scenes, each with their own dedicated page in the documentation:
				- Added a new example scene: \ref example_recast3d
					\shadowimage{generated/scenes/Recast3D/overview.png}
				- Added a new example scene: \ref example_recast_doors
					\shadowimage{generated/scenes/RecastDoors/overview.png}
				- Added a new example scene: \ref example_recast2d
					\shadowimage{generated/scenes/Recast2D/overview.png}
				- Added a new example scene: \ref example_recast_offmeshlinks
					\video{generated/scenes/RecastOffMeshLinks/interact.webm}
				- Added a new example scene: \ref example_recast_tags
					\shadowimage{generated/scenes/RecastTags/overview.png}
				- Added a new example scene: \ref example_recast_terrain
					\shadowimage{generated/scenes/RecastTerrain/overview.png}
				- Added a documentation page for the turn based example scene: \ref example_turn_based
					\shadowimage{generated/scenes/HexagonalTurnBased/overview.png}
				- Improved the infinite world example scene with nicer meshes: \ref example_infinite_world
					\shadowimage{generated/scenes/InfiniteWorld/overview.png}
				- Improved documentation and look of the path types example scene: \ref example_path_types
					\shadowimage{generated/scenes/PathTypes/multitargetpath.png}
				- Improved documentation and look of the moving example scene: \ref example_recast_moving
					\shadowimage{generated/scenes/RecastMoving/overview.png}
			- Added a flowchart to help new users find the right graph type for their game. See \ref graph-tldr.
		- Improved accuracy of path searches on recast/navmesh graphs significantly.
			- Previously, paths on recast/navmesh graphs have been searched by going from triangle center to triangle center. This is not very accurate
			and can lead to agents choosing suboptimal paths.
			Now, paths use a more accurate method which moves between different points on the sides of the triangles instead of the centers.
			This is much more accurate and should lead to agents choosing better paths in almost all cases.
			- It also accounts for the exact start and end point of the path more accurately now.
			- However, this improved accuracy comes with a cost, and pathfinding performance on navmesh/recast graphs is a bit lower than before.
			Luckily, it is still very fast, and most games are not bottlenecked by this.
		- Recast graphs now treat convex colliders (box, sphere, capsule and convex mesh colliders) are as solid, and will no longer generate a navmesh inside of them.
		- Recast graphs now have better support for trees
			- Colliders on child objects will also be taken into account.
			- You can use the \reflink{RecastNavmeshModifier} component on trees to cutomize how they are rasterized.
			- Significantly improved performance when you have many trees.
			- Tree rotation is now taken into account.
		- You can now mark surfaces with specific tags, for recast graphs.
			Just add the \reflink{RecastNavmeshModifier} component to an object and set the mode to \reflink{RecastNavmeshModifier.Mode.WalkableSurfaceWithTag}.
		- \reflink{NavmeshCut}s now support some 3D shapes (box/sphere/capsule) in addition to the previous 2D shapes (rectangle/circle) which makes them a lot more pleasant to use.
			- \video{generated/scenes/MiscVideos/navmeshcutshapes.webm}
		- \reflink{NavmeshCut}s now support expanding the cut by the agent radius. Making them more useful if you have multiple graphs for different agent sizes.
		- The recast graph now supports terrain holes.
		- Added support for pathfinding, local avoidance and movement on spherical worlds, and other strange world shapes. See \ref spherical.
		- Added 2D support for the recast graph.
			The recast graph now supports 2D colliders and the inspector has a new enum that allows you to change between 2D and 3D mode.
			There's a new field \reflink{RecastGraph.backgroundTraversability} that controls if a navmesh should be generated for an unobstructed background in 2D mode.
		- Improved consistency guarantees when doing an async graph scan.
			Previously, the graphs could be in partially scanned states during the scan, which could cause e.g. GetNearest calls to return incorrect results, or throw exceptions.
			Now, the graphs will be in a consistent state at all times during the scan (from the main thread's perspective).
		- Added icons to many components. This should make it easier to find the right component in the inspector.
			\shadowimage{changelog/component_icons.png}
		- Added \reflink{NavmeshPrefab} to allow easily storing recast graph tiles in prefabs and load them at runtime (great for big procedurally generated levels).
		- The LayeredGridGraph now supports 8 neighbours per node instead of 4 as was the case previously.
		- You can now right-click on almost any property in a component, and get a link directly to the online-documentation for it.
			This existed in some old versions too, but it had to be removed due to Unity limitations. But now it’s back again!
		- When hovering over properties in the Unity inspector, you now get much better tooltips on most components.
- <b>New Features and Improvements</b>
		- Added \reflink{NodeLink2.graphMask}, which allows you to set which graphs a link is allowed to connect.
		- Added \reflink{GridNodeBase.OffsetToConnectionDirection}.
		- Added \reflink{GridNodeBase.CoordinatesInGrid}, which is the vector (\reflink{GridNodeBase.XCoordinateInGrid}, \reflink{GridNodeBase.ZCoordinateInGrid}), but calculated in a more optimized way.
		- Added support for triggering an \reflink{Interactable} component using an off-mesh link.
		- Added \reflink{NodeLink2.onTraverseOffMeshLink} to allow custom traversal logic for specific off-mesh links.
		- \reflink{NodeLink2} now automatically updates the graph connections when the links is moved around, even when not in play mode.
		- \reflink{NodeLink2}'s gizmos are now aligned to the graph it is connected to.
		- Improved performance when doing small graph updates on a recast graph, when there's a large terrain in the scene.
		- Optimized graph updates on recast graph so that if multiple graph updates are scheduled that try to update the same tiles, it will try to avoid doing duplicate work.
			This can significantly improve performance when using e.g. the \reflink{DynamicObstacle} component.
		- Reworked how one-way links are handled internally.
			This fixes a number of issues with one-way links, that could lead to exceptions or pathfinding just not working.
			Even for a one-way link, the target node now also knows that it has a connection to the source node.
		- Added \reflink{GraphNode.AddPartialConnection} and \reflink{GraphNode.RemovePartialConnection} for the cases where you need low-level connection modifications.
		- Added a filter parameter to \reflink{GraphNode.GetConnections} to allow excluding incoming or outgoing connections. By default it only includes outgoing connections.
		- Added a link to the online documentation when right-clicking on fields in the Unity inspector (supported for most, but not all, fields).
			This was removed a while ago due to Unity limitations, but it is now back!
		- Added better tooltips for most components.
		- Added \reflink{NavGraph.bounds}.
		- Added a warning if the input mesh to a navmesh graph contains degenerate triangles. These triangles are now also removed automatically, instead of causing errors down the line.
		- Added \reflink{GridGraph.SetWalkability} as a helper function to set walkability for many nodes at once.
		- Added \reflink{IAstarAI.movementPlane}.
		- Off-mesh links are now never serialized with the graphs. Instead, the NodeLink2 component will always recreate them when the scene is loaded.
			This significantly reduces ways things can get out of sync.
		- Off-mesh links are now smarter about when they recalculate their connections due to graph updates. This resolves several previous issues in which off-mesh links could be broken by graph updates.
		- Significantly refactored how graphs are updated and scanned.
			All graph updates now guarantee that any updates will be done atomically from the main-thread's perspective.
			Previously, in-progress graph updates could in some cases be visible, leading to weird results from e.g. the GetNearest method.
			It could also lead to exceptions when using the FollowerEntity component.
		- Graphs are now scanned and updated concurrently.
			Previously, only one graph could be scanned or updated at a time (even if they might have used parallelism internally).
		- Added a new example movement behavior: \reflink{MoveInCircle}.
		- Improve graph update performance when there are lots of dynamic \reflink{RecastNavmeshModifier} components in the scene.
		- The documentation now includes screenshots of the inspector for all components in the package.
		- Added \reflink{AstarPath.OnPathsCalculated}.
		- Added \reflink{GraphDebugMode.NavmeshBorderObstacles}.
		- Clarified documentation on \reflink{GraphHitInfo}'s fields, about what their values are when no obstacle was hit.
		- Added ASTAR_MORE_MULTI_TARGET_PATH_TARGETS to the A* Inspector -> Optimizations tab. It allows you to increase the maximum number of targets for a multi target path from around 256 to around 4096.
		- Reduced likelyhood of gaps being generated between tiles in recast graphs. This will reduce the precision of the navmesh a bit on the y-axis along tile edges, but agents should be able to navigate better (and it will look better).
		- Combined several methods in the layered grid graph and regular grid graph to use the same implementation, reducing code duplication and improving compile times.
		- Added \reflink{GridGraph.RecalculateConnectionsInRegion}.
		- Added \reflink{GridGraph.RecalculateAllConnections}.
		- The grid graph inspector will now list which tags will be used for erosion, when the tags option is enabled.
		- You can now choose which tags erosion is allowed to overwrite, when the tags option is enabled, using the new \reflink{GridGraph.erosionTagsPrecedenceMask} option.
		- Changed the behavior of the recast graph's collider detail field. Previously it would not scale with the cell size, which made it hard to transfer between scenes of different scales.
		Now it scales with the cell size, so leaving it at the default value of 1 should work well for almost all games.
		- The recast graph's center, size and rotation fields will now be rounded to a multiple of 0.5 if it is very close to one.
		- Added \reflink{AIDestinationSetter.useRotation}, which is usable with the \reflink{FollowerEntity} movement script.
			\video{generated/scenes/Recast3D/facingdirection.webm}
		- Navmesh graphs now have an option to expand all navmesh cuts by a constant amount.
		- Simplified the code for path types significantly. There's now a lot more code sharing.
		- Improved the \reflink{MultiTargetPath}.
			- It now properly properly works even with multiple targets within a single node. This is particularly important for accuracy on navmesh graphs.
			Previously all endpoints would be snapped to the closest node center.
			- Improved performance when only finding the closest target (\reflink{MultiTargetPath.pathsForAll}=false), sometimes significantly so.
			- Removed the MultiTargetPath.heuristicMode field, it now automatically does the best thing.
		- Added an overload of \reflink{GraphNode.GetConnections} which takes a ref value that will be passed to the callback. This can be used to avoid allocations when getting connections due to not having to allocate a closure.
		- \reflink{ABPath} now accepts an ending condition (\reflink{ABPath.endingCondition}), making the XPath path type obsolete.
		- Added \reflink{NodeLink2.pathfindingTag} to be able to control which agents can traverse an off-mesh link.
		- Added the \reflink{PathfindingTag} struct. You can add this to your scripts to automatically get a nice dropdown in the inspector for selecting pathfinding tags.
		- Added \reflink{GraphNode.ContainsPoint(Vector3)} and \reflink{GraphNode.ContainsPointInGraphSpace}.
		- Added \reflink{GridNodeBase.ContainsPoint} and \reflink{GridNodeBase.ContainsPointInGraphSpace}.
		- \reflink{RecastNavmeshModifier} components now have a "Geometry Source" field, so that you can choose manually if you want to use a mesh or a collider when voxelizing it.
		- \reflink{RecastNavmeshModifier} components now have a "Include In Scan" field, which allows you to choose if the object should be filtered using layer/tag masks, or if it should always/never be included in the scan.
			Previously RecastNavmeshModifier components were always included in the scan. When upgrading from, all existing RecastNavmeshModifier components will be set to "AlwaysInclude" for compatibility.
		- Added support for scheduling paths (\reflink{AstarPath.StartPath}) from within the Unity Job System.
		- Allow running nearest node queries and other node lookup functions from within the unity job system.
			This requires you to call the \reflink{AstarPath.LockGraphDataForReading} function to ensure safety.
		- Reduced the overhead of having many \reflink{AIDestinationSetter} components.
		- Added a new distance metric when searching for the closest node: \reflink{DistanceMetric.ClosestAsSeenFromAboveSoft(Vector3)}.
			This is much nicer for character movement in most cases, and also usually faster than the default euclidean metric.
			\shadowimage{distance_metric/distance_metric_manhattan_soft.png}
		- Improved performance when searching for nearest nodes on navmesh/recast graphs (sometimes up to 5x, but typically only by a small percentage).
		- Improved rebuild performance of the recast/navmesh internal bounding box tree (particularly important when using navmesh cutting).
		- Improved accuracy of GetNearest calls on grid graphs. Previously the returned node was not necessarily the closest one if \reflink{AstarPath.fullGetNearestSearch} was not enabled.
		- Added \reflink{RecastGraph.Resize} to allow changing the number of tiles of a graph during runtime.
		- Added \reflink{RecastGraph.ReplaceTiles} to allow replacing multiple tiles at once.
		- Added \reflink{ITraversalProvider.filterDiagonalGridConnections} for better control over how diagonal grid connections are handled.
		- Added a warning to the inspector when scanning a recast graph that includes unreadable meshes.
			Previously a warning would only be logged to the console.
			\shadowimage{changelog/recast_unreadable_warning.png}
		- Implemented a new and better algorithm for path simplification on grid graphs based on the paper "Toward a String-Pulling Approach to Path Smoothing on Grid Graphs".
		This essentially makes the raycast modifier obsolete for grid graphs (unless you use the physics raycasting option) as the new algorithm is both faster and more accurate.
		It is used automatically by the \reflink{FunnelModifier} on grid graphs.
		It also takes penalties/tags into account, which has been a long requested feature.
		- Improved handling of tags and \reflink{ITraversalProvider}s for grid graphs.
			Previously when a path was calculated it would only take tag walkability and ITraversalProviders into account in a very basic way.
			For example normally an agent is not allowed to move diagonally between two unwalkable nodes, even if both the start and end nodes are themselves walkable.
			However this check did not apply to tags and ITraversalProviders because that info was precalculated and tags/ITraversalProviders can be set per path.
			Now this is properly taken into account.
			This also means things like the \reflink{GridGraph.cutCorners} option will apply to tags and ITraversalProviders.
			See \reflink{GridNode.FilterDiagonalConnections} for more details.
		- Changed when the AIPath/RichAI movement scripts stop when \reflink{AIBase.whenCloseToDestination} is set to Stop.
			Previously it would stop up to \reflink{AIBase.endReachedDistance} units from the end of the path.
			end up within \reflink{AIBase.endReachedDistance} units from the destination.
			Essentially \reflink{AIBase.reachedDestination} would remain false even though the agent could actually reach the destination by just moving a bit further.
			So this behavior has been changed so that it now only stops when it is within \reflink{AIBase.endReachedDistance} units from the destination, not the end of the path.
		- The \reflink{RecastNavmeshModifier} now has an option for making a mesh solid. This is useful to prevent a navmesh from being generated inside objects.
		- Added \reflink{Pathfinding.ITraversalProvider;ITraversalProvider.CanTraverse(path,from,to)}.
			This allows you to control not only which nodes are traversable, but also which connections can be used.
		- The \reflink{ProceduralGraphMover} script now has an option for specifying which graph to update.
		- Added a mode for the RecastNavmeshModifier to make objects be completely ignored when scanning recast graphs.
			This is useful if you are running out of layers in your project.
		- Added \reflink{GridNodeBase.HasAnyGridConnections}.
		- Removed LayerGridGraph.mergeSpanRange. This was a rarely used and hard to understand setting. Now it is automatically set to half the LayerGridGraph.characterHeight.
		- Improved performance when scanning graphs in the editor outside of play mode by not searching through assemblies for graph types every time.
		- Made \reflink{AstarData.ClearGraphs} public.
		- Added a helper window with shortcuts when the GraphUpdateScene object is selected and the Move tool is active.
		- When GraphModifier events and some callbacks like AstarPath.OnGraphsUpdated are actually fired has been inconsistent and not very well (or accurately) documented for a long time.
			This release makes it more consistent and better documented. Note that this is a breaking change so if you have used any of those callbacks in your code
			you may want to have a look at the #Pathfinding.GraphModifier documentation which describes when all events are fired.
		- Added an \reflink{FunnelModifier.quality} option to try harder to simplify the path when using the FunnelModifier.
		- To be more descriptive all modifiers have had their visible names changed to include "Modifier" (unless it was already included).
			This is the same type of simplification that the \reflink{RichAI.funnelSimplification} option is using.
		- Added the property \reflink{IAstarAI.desiredVelocityWithoutLocalAvoidance} to the AIPath and RichAI components.
			This allows you to both read and write the internal velocity of the agent.
		- The recast graph's position/rotation and size can now be edited using handles in the scene view.
		- Improved rotation filtering for AIPath/RichAI components.
			This reduces jitter in the agent's rotation when they are close to standing still. This is particularly important when using local avoidance.
		- Greatly improved the performance of scanning a grid graph with a high erosion setting.
		- The RVOSimulator inspector has been improved visually.
		- Local avoidance now supports more aggressive collision prevention.
			This is controlled by the \link Pathfinding.RVO.RVOSimulator.hardCollisions RVOSimulator.hardCollisions\endlink field.
			Enabling this will cause the system to try very hard to avoid any kind of agent overlap, similar to how a physics engine would do it.
			This does not influence agent avoidance when the agents are not overlapping.
		- RichAI now has a \reflink{RichAI.whenCloseToDestination} field which behaves like the one that the AIPath script has had for some time.
		- AIPath/RichAI now have options for more intelligently stopping earlier if there are already a lot of agents around the destination (see #Pathfinding.AIBase.rvoDensityBehavior).
			This prevents the agents from walking around endlessly trying to reach the destination when many agents have the same destination.
			This can only be used together with the local avoidance system, i.e. when there's also an RVOController attached to the character.
		- The \reflink{RandomPath} will now select a random point on the end node, instead of always using the center.
			If you want to use the center, set Seeker -> Start End Modifier -> End Point Snapping to Node Center.
		- \reflink{GraphNode.RandomPointOnSurface} is now thread-safe.
- <b>Breaking changes</b>
		- Take a look at \ref upgrading, for help on how to upgrade from 4.2.x to 5.0.
		- The example script \reflink{MineBotAnimation} has been changed to normalize the NormalizedSpeed parameter it sends to the animator by a new parameter called \reflink{MineBotAnimation.naturalSpeed}.
		- Renamed the ProceduralGridMover component to \reflink{ProceduralGraphMover}, since it now supports more than just grids.
		- Changed navmesh/recast graph linecasts to return a hit if the linecast started outside the graph.
			Previously, if doing a linecast from inside the graph to the outside, it would return a hit, but a linecast from the outside to the inside would return no hit.
			This was a bug, but it's been present for so long that at this point it's basically a breaking change.
		- The built-in movement scripts no longer listen for \a all paths that their attached Seeker component calculates, instead they only listen for paths that they request.
			If you have been relying on this by calling seeker.StartPath from another script, and expecting the movement scripts to start to follow the path, you will need to change this to call \reflink{IAstarAI.SetPath;ai.SetPath} instead.
			This has been the recommended way to do it for a long time, but now it is required.
			The Seeker component will log a warning if a path is calculated but no script is listening for its completion.
		- Namespace structure has been significantly improved. I have tried to keep most changes to internal classes that are not used often by users, but you may have to adjust your \a using statements slightly.
			Here's a summary of the changes:
			- \reflink{Pathfinding.Graphs.Grid} has been added, and a lot of classes related to the grid graph have been moved there.
			- \reflink{Pathfinding.Graphs.Grid.Jobs} is new and contains a lot of internal grid scanning code.
			- \reflink{Pathfinding.Graphs.Grid.Rules} is new, and contains grid graph rules.
			- \reflink{Pathfinding.Graphs.Navmesh} replaces Pathfinding.Recast.
			- \reflink{Pathfinding.Graphs.Navmesh.Voxelization} replaces Pathfinding.Voxels.
			Sadly C# doesn't support public namespace aliases, so these changes cannot be made backwards compatible.
		- If you have written your own graph type (one of a rare bunch), you may need to update your scanning code slightly.
			The \reflink{NavGraph.ScanInternal} method should now return a \reflink{IGraphUpdatePromise}.
			Take a look at \ref writing-graph-generators for more details.
		- Scanning when the game starts now happens during early \a OnEnable instead of during early \a Awake.
			This should typically not cause any issues, since it's good practice in Unity to not depend on other components being initialized in \a Awake.
			But this may cause issues if you have been depending on this very specific initialization order.
		- The type of some tag fields have changed from int to \reflink{PathfindingTag}. If you have assigned a tag to those fields before, you will now first need to convert it to a PathfindingTag: \code new PathfindingTag((uint)tag)\endcode.
		- Removed support for deserializing graphs from version 3.8.x and below. 3.8.x was released over 8 years ago, so this should hopefully have given people enough time to upgrade.
		- \reflink{NavGraph.GetNearest} will now always take the constraint into account. Previously this depended on the graph type and it was generally hard to use to get consistent results across graph types.
		- \reflink{NavGraph.GetNearest} now returns an \reflink{NNInfo} instead of a NNInfoInternal.
		- The \reflink{PathInterpolator} class has been refactored significantly and now represents points using a Cursor struct, instead of storing it on the interpolator itself.
			Normally the \reflink{PathInterpolator} is not used by users, but a few may have used it in their own movement scripts.
			In that's the case for you, then you will need to make small modifications to your code to create a cursor representing the current point.
		- Removed support for local avoidance obstacles (\reflink{RVOSquareObstacle}).
			Local avoidance obstacles never worked particularly well and the performance was poor.
			Use regular graph updates to update the pathfinding graphs instead.
		- The \reflink{RVONavmesh} component has been deprecated. This is now handled automatically by enabling the \reflink{RVOSimulator.useNavmeshAsObstacle} option.
		- GridGraph.nodes now has the type GridNodeBase[] instead of GridNode[]. The same is true for the layered grid graph.
			In most cases everything should work as normal, however if you have been accessing GridNode specific methods or properties on a node you may need to cast the node to a GridNode (or LevelGridNode) to avoid compile errors.
		- The LayeredGridGraph now only supports 15 layers by default. This is to reduce memory usage for the common case and to reduce code complexity. If this causes issues for your workflow, please let me know in the forum.
		- Due to local avoidance now supporting non-planar worlds many internal coordinates have been changed from float2 to float3.
			If you have been using the RVOController script then you do not need to do anything, however if you have used the internal IAgent interface
			then you may have to change your code a bit to pass 3D coordiantes instead of 2D ones.
		- GraphModifier events and some callbacks on the AstarPath object (e.g. OnGraphsUpdated) have changed semantics. Read below for more info.
		- OnPostUpdate no longer runs during scan.
		- Made GridGraph.neighbourGridOffsetsX and neighbourGridOffsetsZ to static readonly members since they data is constant.
		- Graph updates for grid graphs no longer call the GraphUpdateObject.Apply method, instead the GraphUpdateObject.ApplyJob method is called.
			If you have been inheriting from the GraphUpdateObject to make custom graph updates you may need to convert your code so that it works with Burst.
		- Fixed hexagon connection costs were sqrt(3/2)≈1.22 times too large.
			The connection costs are now approximately 1000*the distance between the nodes, as it should be.
			For example if you have set the hexagon width to 1 then the cost to move between two adjacent hexagons is now 1000 instead of 1224 like it was before.
			For almost all users this will not affect anything, however it may improve pathfinding performance a bit.
			If you have been using penalties on your graph then you may have to divide them by 1.22 to get the same behavior.
			Similarly if you have been using the ConstantPath to get a set of nodes you may have to divide the maxGScore parameter by 1.22.
		- The AIPath and RichAI scripts no longer use the Update and FixedUpdate methods.
			Instead a separate script (\reflink{BatchedEvents}) is used which allows all components of a specific type to be processed at once.
			This is slightly faster and is also required for the \link Pathfinding.AIBase.rvoDensityBehavior AIBase.rvoDensityBehavior\endlink implementation.
			If you have been overriding the Update or FixedUpdate methods you will need to change your code to instead override the OnUpdate method.
		- Local avoidance has hard collisions enabled by default. If this is not what you want you can disable this in the RVOSimulator component settings. See below for more details.
		- Changed \reflink{IAstarAI.endOfPath} to return the agent's current position if the agent has no path, and no destination is set (instead of (+inf,+inf,+inf)).
- <b>Changes</b>
		- The AstarPath component will now initialize itself during early \a OnEnable instead of during early \a Awake.
			This makes it possible to set custom settings on the AstarPath component, such as the thread count, from a script when the game starts.
			Previously this was kinda tricky, since no user scripts would have been run before the AstarPath component initialization.
		- Removed \reflink{AstarPath.prioritizeGraphs}. It was always a bit of a hack. Use \reflink{NNConstraint.graphMask} if you want to choose which graphs are searched.
		- Removed \reflink{AstarPath.fullGetNearestSearch}. It is now always treated as true.
		- Removed double buffering support from the local avoidance system.
			Double buffering was causing issues with newer features, and it didn't give that much benefit anyway (and caused agents to be less responsive).
			The system can already simulate up to 30000 agents at 60 fps, so the use cases were limited.
		- Renamed NNConstraint.Default to \reflink{NNConstraint.Walkable} for clarity.
		- Deprecated a lot of the older example scenes. They have been moved into the OldExamples folder.
		- Renamed GraphNode.ContainsConnection to \reflink{GraphNode.ContainsOutgoingConnection} to distinguish incoming from outgoing connections.
		- Deprecated GraphNode.AddConnection and GraphNode.RemoveConnection. Use the static methods \reflink{GraphNode.Connect} and \reflink{GraphNode.Disconnect} instead.
		- Deprecated \reflink{GraphUpdateObject.trackChangedNodes}. \reflink{AstarPath.Snapshot} will be the replacement for this.
		- Deprecated \reflink{GraphUpdateObject.RevertFromBackup}. \reflink{AstarPath.Snapshot} will be the replacement for this.
		- Deprecated GridGraph.CalculateConnections(int,int). Use \reflink{GridGraph.RecalculateConnectionsInRegion} instead to batch multiple calls.
			This method still works, but the overhead of calling it will be significantly higher than in earlier versions.
			This new overhead comes from properly handling the new \reflink{GridGraph.maxStepUsesSlope} option, and job scheduling overhead.
			If you are recalculating connections for a large number of nodes, you should use \reflink{GridGraph.RecalculateConnectionsInRegion} instead.
			The RecalculateConnectionsInRegion method scales better to larger node counts than CalculateConnections ever did.
		- Deprecated \reflink{XPath}. Use \reflink{ABPath} with an ending condition instead.
		- Removed Jump Point Search support for grid graphs. It was quite limited, and very few people seemed to use it. But it contributed a significant amount of code.
		- Removed GraphCollision.rayDirection. This option has never been exposed in the inspector and it's not that useful.
		- Deprecated \reflink{NavmeshBase.nearestSearchOnlyXZ}. Use \reflink{NNConstraint.distanceMetric} instead.
		- Deprecated \reflink{NNConstraint.distanceXZ}. Use \reflink{NNConstraint.distanceMetric} instead.
		- Deprecated \reflink{NavGraph.GetNearestForce}. Use \reflink{NavGraph.GetNearest} instead, as it does the same thing now.
		- Deprecated the AIPath/RichAI.repathRate field. It has been replaced by the \reflink{AIBase.autoRepath.period} field.
		- Removed a lot of deprecated code. All of these have been deprecated for at least a few years.
			- Removed previously deprecated members of the NavGraph class: matrix, inverseMatrix, SetMatrix, RelocateNodes(Matrix4x4,Matrix4x4).
			- Removed previously deprecated method NavGraph.ScanGraph, use \reflink{NavGraph.Scan} instead.
			- Removed previously deprecated property RecastGraph.forcedBounds.
			- Removed previously deprecated method RecastGraph.ClosestPointOnNode (use node.ClosestPointOnNode instead).
			- Removed previously deprecated method RecastGraph.ContainsPoint (use node.ContainsPoint instead).
			- Removed previously deprecated method GridGraph.CheckConnection (use node.HasConnectionInDirection instead).
			- Removed previously deprecated method GridGraph.GetNodesInArea (use \reflink{GridGraph.GetNodesInRegion} instead).
			- Removed previously deprecated method overload GridGraph.CalculateConnections(int,int,GridNode).
			- Removed previously deprecated method GridGraph.UpdateNodePositionCollision.
			- Removed previously deprecated method GridGraph.UpdateSizeFromWidthDepth (use \reflink{GridGraph.SetDimensions} instead).
			- Removed previously deprecated method GridGraph.GenerateMatrix (use \reflink{GridGraph.UpdateTransform} instead).
			- Removed previously deprecated method GridGraph.GetNodeConnection (use \reflink{GridNodeBase.GetNeighbourAlongDirection} instead).
			- Removed previously deprecated method GridGraph.HasNodeConnection (use \reflink{GridNodeBase.HasConnectionInDirection} instead).
			- Removed previously deprecated method GridGraph.SetNodeConnection (use \reflink{GridNode.SetConnectionInternal} instead).
			- Removed previously deprecated class LegacyAIPath, as it's been deprecated for about 6 years now.
			- Removed previously deprecated class LegacyRichAI, as it's been deprecated for about 6 years now.
			- Removed previously deprecated class LegacyRVOController, as it's been deprecated for about 6 years now.
			- Removed previously deprecated method Seeker.GetNewPath (use \reflink{ABPath.Construct} instead).
			- Removed previously deprecated overload of Seeker.StartMultiTargetPath (use \reflink{Seeker.StartPath} instead).
			- Removed previously deprecated field AstarPath.astarData (use \reflink{AstarPath.data} instead).
			- Removed previously deprecated field AstarPath.limitGraphUpdates (use \reflink{AstarPath.batchGraphUpdates} instead).
			- Removed previously deprecated field AstarPath.maxGraphUpdateFreq (use \reflink{AstarPath.graphUpdateBatchingInterval} instead).
			- Removed previously deprecated property AstarPath.IsAnyGraphUpdatesQueued (use \reflink{AstarPath.IsAnyGraphUpdateQueued} instead).
			- Removed previously deprecated callback AstarPath.OnGraphsWillBeUpdated.
			- Removed previously deprecated callback AstarPath.OnGraphsWillBeUpdated2.
			- Removed previously deprecated method AstarPath.QueueWorkItemFloodFill.
			- Removed previously deprecated method AstarPath.EnsureValidFloodFill.
			- Removed previously deprecated method AstarPath.FloodFill.
			- Removed previously deprecated method AstarPath.FlushWorkItems(bool,bool) (use \reflink{AstarPath.FlushWorkItems()} instead).
			- Removed previously deprecated method AstarPath.FlushThreadSafeCallbacks (use \reflink{AstarPath.FlushWorkItems()} instead).
			- Removed previously deprecated method AstarPath.BlockUntilPathQueueBlocked (use \reflink{AstarPath.PausePathfinding} instead).
			- Removed previously deprecated method AstarPath.WaitForPath (use \reflink{AstarPath.BlockUntilCalculated} instead).
			- Removed previously deprecated method AstarPath.RegisterSafeUpdate (use \reflink{AstarPath.AddWorkItem} instead).
			- Removed previously deprecated method GraphNode.RecalculateConnectionCosts.
			- Removed previously deprecated component TileHandlerHelper, as this is built-in to navmesh/recast graphs now.
		- \reflink{PointGraph.optimizeForSparseGraph} is now included in the free version too. This option makes scanning a large point graph much faster.
		- Changed some private fields in \reflink{AIPath} to be protected instead, to make it easier to subclass it.
		- Changed the signature of GraphNode.GetPortal to use out-parameters instead of output lists. The old signature will continue to work, but it is marked as deprecated.
		- Moved ProceduralGraphMover and DynamicObstacle out from the ExampleScenes folder to the Utilities folder.
		- Change: \reflink{LevelGridNode.HasAnyGridConnections} is now a property instead of a method.
		- Renamed GridGraph.maxClimb to the more descriptive name \reflink{Pathfinding.GridGraph.maxStepHeight}.
- <b>Fixes</b>
		- Fixed the AIPath.velocity and RichAI.velocity properties being very jittery (and often completely incorrect) when a rigidbody was attached to the character.
		- Fixed linecasts on grid graphs would not add the start node to the \a trace parameter if the end node was the same as the start node.
		- Fixed enabling \reflink{AIPath.constrainInsideGraph} could cause the agent to be teleported to the world origin if it was too far away from any walkable nodes.
		- Fixed the AILerp component would recalculate the path every frame if the agent's position, the destination, and the destination of the last calculated path were all identical.
		- Fixed the \reflink{NavMeshGraph} would not work for some pretty bad meshes.
		- Fixed some deprecation warnings in newer versions of Unity.
		- Added \reflink{OffMeshLinks.GetNearest}.
		- The navmesh graph will now warn if the input mesh contains duplicate triangle edges, instead of crashing.
		- Fixed \reflink{AstarPath.FlushGraphUpdates} and \reflink{FlushWorkItems} would not flush items if some were in progress, but none were queued.
		- Improved error handling for \reflink{RecastNavmeshModifier} when it is moved during runtime.
		- Fixed paths contained some internal fields which made it impossible to write custom path types.
		- Fixed some linecast edge cases on grid graphs in which the \reflink{GraphHitInfo}'s fields were not set correctly.
		- Fixed the recast graph's 'Snap bounds to scene' button could misalign the graph if the graph was rotated.
		- Fixed rotated \reflink{RecastGraph}s could in some cases end up with gaps between tiles because some meshes that should have been included in the tile were missed.
		- The \reflink{RVOController} will now disable itself if the global \reflink{RVOSimulator} is disabled.
		- Fixed GridGraph and LayerGridGraph not always using \reflink{GridGraph.newGridNodeDelegate} when creating nodes.
		- Fixed editing \reflink{NavmeshCut} components in a scene without an AstarPath component would throw an exception.
		- Fixed "Some meshes were statically batched..." error message being logged multiple times when scanning a recast graph. Now it is limited to at most once per scan.
		- Fixed RVOExampleAgent could throw an exception if it tried to follow a path with length 1.
		- Fixed exception that could be thrown when using local avoidance and the navmesh had degenerate edges (could happen when using the \reflink{NavmeshAdd} component).
		- Fixed graph updates on grid graphs sometimes updated a slightly larger number of nodes than strictly necessary.
		- Fixed the \reflink{FunnelModifier} could simplify the path incorrectly on rotated recast graphs.
		- Fixed setting \reflink{RichAI.rotation} while the agent was stationary would rotate the agent, but it would immediately rotate back to the previous heading.
		- Fixed an exception could be thrown when using the \reflink{NavmeshClamp} script with multiple graphs.
		- Fixed the \reflink{RecastNavmeshModifier} component would sometimes still affect objects even if it was disabled.
		- Fixed \reflink{ProceduralGraphMover} not working with layered grid graphs.
		- Fixed the graphs would be rendered even when the scene view was in prefab isolation mode and you were viewing an unrelated prefab.
		- Improved error messages when trying to include a non-readable mesh in a recast graph scan.
		- Cleaned up some large audio files in the example scenes to reduce the package install size quite significantly.
		- Removed some `.blend1` (blender backup files) that had been accidentally included in the package.
		- Fixed some visual scaling bugs in the collision visualization code for the grid graph.
		- Worked around a null reference exception in Unity's ClearPreviewCache that could appear randomly from time to time.
		- Fixed RichAI always teleporting to the closest point on the navmesh when the script is enabled even if \reflink{AIBase.canMove} is false.
		- Fixed a crash when running on iOS.
		- Fixed AIPath/RichAI throwing exceptions when an RVOController on the same object was disabled.
		- Fixed paths could sometimes be cancelled without a reason if 'draw gizmos' was disabled on the Seeker component.
		- Fixed implementations of Object.Equals for Int3, Int2, IntRect, Connection and SimpleMovementPlane would throw an exception if compared to an object of a different type.
		- Fixed an exception could be thrown when resizing a grid graph to make it smaller.
		- Fixed RVOController.layer showing up as a layer mask instead of a single layer field.
		- Fixed a null reference exception which could happen in rare cases when using one-way links on point graphs.
		- Fixed graph updates not always picking up physics changes done during the same frame as the update if "Auto Sync Transforms" has been disabled in the Unity physics settings.
		- Fixed changing the scene and scanning a graph in the same frame may cause the graph to not pick up the latest scene changes if <a href="https://docs.unity3d.com/ScriptReference/Physics-autoSyncTransforms.html">Physics.autoSyncTransforms</a> is disabled.
		- Fixed null reference exception when a navmesh/recast graph is marked as not being affected by navmesh cuts.
		- Fixed collider faces with the exact same y coordinate as the base of a grid graph would sometimes not get detected.
		- Fixed AstarPath.OnGraphsUpdated not being called after NavmeshCut updates.
		- Fixed GraphModifier.OnGraphsPreUpdate could be called multiple times per update when using navmesh cutting.
		- Fixed GraphModifier.OnGraphsPreUpdate could be called in the middle of a graph scan. Now graph scans will only send PreScan, PostScan and LatePostScan events.
		- Fixed weird undo/redo behaviour when editing graph settings. Previously some undo events would not be saved and sometimes you would have to press ctrl+z twice in order to undo a change.

## 4.2.19 (2023-11-14)
- The documentation now has a dropdown to allow you to choose between different versions of the package, and a notification if you are viewing a previous version.
	 	\shadowimage{changelog/documentation_version_dropdown.png}
- Added a new tutorial: \ref traversal_provider.
- You can now set a custom traversal provider on the Seeker component, and it will be applied to all path requests. See \reflink{Seeker.traversalProvider}.
- Changed \link Pathfinding.Seeker.IsDone Seeker.IsDone\endlink to return true from within the OnPathComplete callback.
		Previously it would always return false, which was confusing since the callback explicitly indicates that the path has been calculated.
- Made the grid graph's collision visualization in the inspector green instead of blue.
		This is done to make the connection to colliders more clear. The color is also easier to see against the grey background.
- The \reflink{RaycastModifier} now defaults to graph raycasting in the pro version of the package.
- The transform tool for the GameObject with the AstarPath component is now hidden to reduce clutter, as each graph has its own transform tool instead.
- \reflink{AutoRepathPolicy} will now automatically randomly spread out path recalculations a bit to avoid many agents recalculating their paths at the same time.
- Added \reflink{GraphMask.FromGraphIndex}.
- Added an option for a custom edge filter in \reflink{GraphUtilities.GetContours}.
- Added \reflink{GridGraph.GetNeighbourDirections}.
- Added \reflink{NavGraph.isScanned}.
- Added \reflink{NavGraph.IsInsideBounds}.
- Added \reflink{GraphNode.ContainsPoint} and \reflink{GraphNode.ContainsPointInGraphSpace}.
- Added \reflink{GridGraph.AlignToTilemap}.
- Added \reflink{IAstarAI.endOfPath}.
- Added \reflink{GridNodeBase.HasConnectionsToAllAxisAlignedNeighbours}.
- Added \reflink{GraphUpdateScene.GetGraphUpdate}.
- Added \reflink{ABPath.cost} for accessing the total cost of a calculated path.
- Added a way to align a grid graph to a tilemap. Previously this has been quite tricky, especially for isometric and hexagonal tilemap layouts.
		\shadowimage{gridgraph/align_to_tilemap.png}
- Added ASTAR_LARGER_GRIDS which can be set under the Optimizations tab to enable grid graphs larger than 1024x1024.
- Changed how \reflink{GridGraph.aspectRatio} works for isometric and hexagonal grid graphs. Now it works more intuitively to just make the nodes N times wider.
		Before it only behaved properly for rectangular grid graphs.
- Deprecated NNConstraint.Default. Use the equivalent \reflink{NNConstraint.Walkable} property instead, as this name is more descriptive.
- Deprecated \reflink{AIPath.OnTargetReached}, \reflink{RichAI.OnTargetReached}, \reflink{AILerp.OnTargetReached}.
		You can use \reflink{IAstarAI.reachedDestination} or \reflink{IAstarAI.reachedEndOfPath} instead.
		The OnTargetReached method will continue to work as before, but using it is discouraged.
- Deprecated the AdvancedSmooth modifier. It never worked particularly well, and given that no one has asked a single support question about it the last 5 years or so, I don't think a lot of people use it.
- Fixed \link Pathfinding.IAstarAI.SetPath ai.SetPath\endlink did not accept paths if you called it from the OnPathComplete callback for the path.
- Fixed the first tag color in the inspector would sometimes be initialized to completely transparent.
- Fixed \reflink{NavmeshCut}s with custom meshes could calculate an incorrect bounding box, which could lead to it not updating some tiles, even if it was touching them.
- Fixed \reflink{GraphUpdateScene.GetBounds} sometimes incorrectly returning an empty bounding box if the \a convex option was enabled.
- Fixed some cases where a recast graph would miss some connections between nodes in different tiles. Especially if the shared edge was long.
- Fixed Navmesh Cutting Update Interval would not be serialized, and therefore always reset to 0 when the game started.
- Fixed the grid graph's collision preview showing the side view when using 2D physics even though that's irrelevant for 2D.
- Fixed an exception could in rare circumstances be thrown when using \reflink{ABPath.calculatePartial}.
- Fixed editor-only data would sometimes not be loaded from graphs, leading to some settings in the graph inspectors to be lost (e.g. if the grid graph's collision preview was open or closed).
- Fixed \reflink{GraphNode.ContainsOutgoingConnection} could throw an exception if the node had no connections at all.
- Fixed calling \reflink{GraphUtilities.GetContours} with a grid graph that had one-way connections could cause an infinite loop.
		Now an exception will be thrown if any one-way connections are found. This is reasonable because the contours of a grid graph are not really well-defined if any one-way connections exist.
- Removed previously deprecated methods on the Path class: GetState, Log, LogError and ReleaseSilent. They have all been deprecated for over 5 years.
- Removed previously deprecated methods on the AstarData class: GetGraphType(string), CreateGraph(string), AddGraph(string, and GetRaycastableGraphs. They have all been deprecated for over 5 years.

## 4.2.18 (2022-11-08)
- Added a documentation page on the architecture of the package: \ref architecture.
- Added a tutorial on migrating from Unity's pathfinding to this package: \ref migrating-from-unity.
- Added a tutorial on how to deal with pathfinding in large worlds: \ref large-worlds.
- Nearest node queries on layered grid graphs are now significantly faster.
- Improved performance of linecasts on grid graphs by approximately a factor of 2.
- Improved accuracy of linecasts on grid graphs. In particular, many edge cases were previously undefined and were not consistently handled.
	  Now linecasts which only touch corners of obstacles are always allowed, and also linecasts which go right on the border between walkable and unwalkable nodes.
- Add \reflink{GridNodeBase.NormalizePoint} and \reflink{GridNodeBase.UnNormalizePoint}.
- Various documentation improvements across the whole package.
- The game view will now automatically repaint after a graph is scanned in the editor. Previously only the scene view was repainted.
- Grid graphs and layered grid graphs now have support for \reflink{NNConstraint.distanceXZ}.
		This makes \reflink{AIPath.constrainInsideGraph} work a lot better for grid graphs which have slopes.
- Changed the color of gizmos drawn for the \reflink{NavmeshAdd} component from green to purple to avoid confusion with the \reflink{Seeker}s gizmos.
- If you have a persistent (DontDestroyOnLoad) AstarPath component while loading a new scene with an existing AstarPath component, the newly loaded AstarPath component
		will disable itself instead of logging an error and leaving both components in inconsistent states. This will make it easier to keep a single AstarPath component
		around for the whole duration of the game.
- A garbage collection will no longer be forced immediately after scanning graphs. Unity has decent support for incremental GC now, and it's best to rely on that instead.
		This may improve performance when scanning graphs.
- Renamed AutoRepathPolicy.interval/maximumInterval to \reflink{AutoRepathPolicy.period}/maximumPeriod since that's a more descriptive name.
- Changed \reflink{AutoRepathPolicy} to make it usable for custom movement scripts without having to implement the \reflink{IAstarAI} interface.
- Increased artificial limit for the maximum length of paths from 2048 to 16384.
- \reflink{GridNodeBase.RemoveConnection} now also works for grid connection, not just custom connections.
- \reflink{GridNodeBase.AddConnection} will now remove an existing grid connection (if it exists) to the same node, before adding the new custom connection.
		Before, you could get two connections between the nodes instead of one, like you'd expect.
- Using \reflink{IAstarAI.SetPath} will now set the destination of the movement script to the end point of the path by default.
		If you pass a \reflink{RandomPath}, \reflink{FleePath} or \reflink{MultiTargetPath}, the destination will be set once the path has been calculated since that's when it is first known.
		This makes properties like \reflink{IAstarAI.reachedDestination} and \reflink{IAstarAI.remainingDistance} work out of the box for those path types.
- Created a property drawer for the \reflink{GraphMask} struct. So now you can include GraphMask fields in your own scripts and they will show up nicely in the inspector.
- Improved error checking in \reflink{FloodPath} for if the start node was destroyed before the path calculation started.
- Fixed saving or loading graphs could deadlock on WebGL since the zip library tired to use threads, which doesn't work on WebGL.
- Fixed linecasts on grid graphs would not always return a hit if the start point or end point was outside the graph.
- Fixed ExitGUIException could show up in the console sometimes when editing component properties.
- Fixed floating point errors causing the \reflink{FunnelModifier} to simplify the path incorrectly in rare cases.
- Fixed not being able to check for updates in Unity 2022.1+ because https must now be used for all web requests.
- Fixed inheriting from scripts in this package (e.g. AIPath) and adding new fields, would not make those fields show up in the inspector unless you had a custom editor script too.
		Now all fields the editor scripts cannot handle will show up automatically.
- Deprecated \reflink{AstarPath.prioritizeGraphs}.
- Deprecated some older linecast overloads for grid graphs.
- Deprecated LevelGridNode.GetConnection in favor of \reflink{LevelGridNode.HasConnectionInDirection}.

## 4.2.17 (2021-11-06)
- Fixed RVO example scenes not working properly (regression introduced in 4.2.16).

## 4.2.16 (2021-10-24)
- Breaking changes
		- Changed when the AIPath movement script stops when \reflink{AIPath.whenCloseToDestination} is set to Stop.
			See below for more details. The new behavior is usually what you want.
			If you really want the old behavior then set \code ai.whenCloseToDestination = CloseToDestinationMode.ContinueToExactDestination\endcode
			and in a separate script run \code ai.isStopped = ai.reachedEndOfPath\endcode every frame.
		- The AIPath script will now clear the path it is following if a path calculation fails.
			See below for more details.
		- Changed the order of execution for the AstarPath script to -10000.
			Previously the order of execution was left at the Unity default value which is very unpredictable.
			This may cause issues for you if you relied on Awake to be executed before the graphs were loaded and scanned.
			If you need full control over when the graphs are scanned it is recommended that you disable \reflink{scanOnStartup} and instead call \reflink{AstarPath.Scan} manually when you want to scan the graphs.
- Improvements
		- Added some helper visualizations for the collision settings for the grid graph.
		\shadowimage{changelog/grid_graph_collision_visualization.png}
		- Improved the way pages are grouped in the documentation to hopefully make it more intuitive.
		- Added \link Pathfinding.GridGraph.SetGridShape GridGraph.SetGridShape\endlink.
		- Added a new dynamic path recalculation mode to the movement scripts.
			This mode is smarter about when it recalculates paths. It will recalculate the path often when the destination changes a lot
			and much less frequently if the destination doesn't change much or if it is very far away.
			This can help improve both the responsiveness of your agents and the performance if you have many agents that do frequent path recalculations.
			Any existing agents will still use the old method of recalculating the path every N seconds, but you can change this in the inspector.
			New agents will by default use the new Dynamic mode.
		- Improved performance of the AIPath and RichAI inspectors a bit.
		- Improved the accuracy and performance of the RaycastModifier when thick raycasting is enabled.
		- The \link Pathfinding.ProceduralGridMover ProceduralGridMover\endlink script now has an option for specifying which graph to update.
		- Added a filter parameter to graph linecast methods. This can be used to mark additional nodes as not being traversable by the linecast.
			See GridGraph.Linecast.
		- The \link Pathfinding.RaycastModifier RaycastModifier\endlink now respects which tags are traversable and any ITraversalProvider set on the path.
		- Added a setter for the rotation property of all movement scripts (\reflink{IAstarAI.rotation}).
		- Exposed \reflink{AILerp.velocity}.
		- Added a get/set property \reflink{GridGraph.is2D} which is equivalent to the inspector's "2D" toggle.
		- Added \reflink{GraphUpdateObject.stage} which contains info about if a graph update has been applied or not.
		- Improved the DynamicObstacle to handle cases when graph updates take a very long time in a better way.
		- Improved the \reflink{NavmeshAdd} component inspector.
		- Added a \reflink{NavmeshClipper.graphMask} field to the \reflink{NavmeshCut} and \reflink{NavmeshAdd} components.
- Changes
		- The AIPath/RichAI.canSearch field has been replaced by the \reflink{AIBase.autoRepath.mode} field.
			For backwards compatibility setting canSearch to false will set the mode to Never and setting it to true will set the mode to EveryNSeconds.
		- The AIPath/RichAI.repathRate field has been replaced by the \reflink{AIBase.autoRepath.period} field.
			For backwards compatibility you can both read and write to the old name and it will work as before.
			Note that this field is not used for the new Dynamic path recalculation mode.
		- The AIPath script will now clear the path it is following if a path calculation fails.
			Previously it would continue following its previous path. This could lead to problems if the world had changed so that there was no longer a valid path to the target.
			The agent could then in some situations just continue trying to walk through obstacles instead of stopping.
			In pretty much all cases this change in behavior is what you want and will not cause any problems when upgrading.
		- Changed when the AIPath movement script stops when \reflink{AIPath.whenCloseToDestination} is set to Stop.
			Consider the case when the destination was slightly outside the navmesh and the path instead goes to the closest point it can reach.
			Previously it would stop up to \reflink{AIPath.endReachedDistance} units from the end of the path.
			This could lead to unexpected behavior since it may be the case that by just moving a bit closer to the end of the path it would
			end up within \reflink{AIPath.endReachedDistance} units from the destination.
			Essentially \reflink{AIPath.reachedDestination} would remain false even though the agent could actually reach the destination by just moving a bit further.
			So this behavior has been changed so that it now only stops when it is within \reflink{AIPath.endReachedDistance} units from the destination, not the end of the path.
		- Made GridGraph.GetRectFromBounds public.
		- Right clicking fields in the inspector no longer shows the "Show in online documentation" context menu, instead the normal unity context menu is shown.
			The previous menu prevented things like resetting individual fields to their prefab values and similar things.
		- Changed some 'internal' access modifiers to 'public' or 'protected' on the Path class. This makes it easier to create custom path types.
- Fixes
		- Fixed a bug which could very rarely cause some node connections between tiles in a recast graph to be missed, causing various subtle movement issues.
		- Fixed \reflink{AIPath.reachedEndOfPath} not always being reset to false when the component was disabled and then enabled again.
		- \reflink{GraphNode.ClosestPointOnNode} is now available for all node types instead of only being implemented for \reflink{GridNode} and \reflink{MeshNode}.
		- Fixed a case where the Seeker -> Start End Modifier -> Snapping to NodeConnection mode would not respect tags or \reflink{ITraversalProvider}s.
			This could result in the path partially entering a node which should not be traversable by it.
		- Fixed a case where using the \reflink{ABPath.calculatePartial} option together with Seeker->Start End Modifier->Snapping=ClosestOnNode would make the path seemingly ignore which tags were traversable in some cases.
		- Fixed clicking Apply in the Optimizations tab could log error messages about some build platforms not existing.
		- Fixed exception when using \reflink{MecanimBridge} with \reflink{RichAI}.
		- Fixed \reflink{AIPath.reachedDestination} would throw an exception if you tried to access it when the agent had no path.
		- Fixed calling `AstarData.ClearGraphs` and then immediately calling `AstarData.AddGraph` would result in an exception.
		- Fixed \reflink{UpdateGraphsNoBlock(GraphUpdateObject,GraphNode,GraphNode,bool)} could return incorrect results if there were already some pending graph updates.
		- Fixed grid graph would see 2D triggers as obstacles. Now all 2D colliders marked as triggers will be ignored by the grid graph.
		- Fixed a memory leak that could happen after switching scenes. Thanks nGenius for finding it.
		- Fixed RichAI always teleporting to the closest point on the navmesh when the script is enabled even if \reflink{RichAI.canMove} is false.
		- Fixed GraphUpdateScene not using the outline from an attached PolygonCollider2D properly.
		- Fixed some cases where graph updates with 2D colliders would not use the most up to date physics engine data.
			All graph updates are now preceeded by a call to Physics2D.SyncTransforms (in addition to Physics.SyncTransform which was already being called).
		- The RaycastModifier now respects graphMasks set on paths. Thanks brettkercher for reporting the bug.
		- Fixed linecasts on navmesh/recast graphs could fail when the start of the line was close to a steep slope.
		- Fixed linecasts on navmesh/recast graphs would incorrectly return no obstructions if for example used between two floors of a building that were not connected.
		- Fixed RichAI trying to traverse an off mesh link twice if the path's endpoint was also the endpoint of an off-mesh link.
		- Fixed a missing script in the turnbased example scene causing warnings when opening that scene.
		- Fixed warnings would be logged when first importing the package on recent version of Unity due to a change in how Unity imports fbx files.
		- Make DynamicObstacle call Physics.SyncTransforms to ensure it has the most up to date data for the collider.
		- Removed 'Upgrading serialized data ...' message as it was mostly just annoying.
		- Fixed GameObject references (like the PointGraph's Root field) would not get serialized properly if the AstarPath component was stored in a prefab.
		- Fixed some smaller memory memory leaks in the unity editor.
		- Fixed a bug which could cause a SynchronizationLockException to be thrown in some cases when the game was quit in a non-graceful way (e.g. when Unity exits play mode after recompiling scripts).

## 4.2.15 (2020-03-30)
- Added \link Pathfinding.IAstarAI.GetRemainingPath ai.GetRemainingPath\endlink.
- Fixed importing the package using the unity package manager would cause the A* inspector to not be able to load since it couldn't find the editor resources folder.
- Fixed the package would report the wrong version number. This could cause the "New Update Available" window to show up unnecessarily.

## 4.2.14 (2020-03-23)
- Fixed DynamicObstacle throwing an exception when scanning sometimes. This bug was introduced in 4.2.13.

## 4.2.13 (2020-03-21)
- Fixed paths could sometimes be cancelled without a reason if 'draw gizmos' was disabled on the Seeker component. Thanks Eran for reporting the bug.
- Fixed DynamicObstacle logging an error about not having a collider attached even outside of play mode.

## 4.2.12 (2020-02-20)
- Fixed "Not allowed to access vertices on mesh" error which some users are seeing after upgrading to Unity 2019.3.

## 4.2.11 (2019-11-28)
- Fixed animations for the agent character were missing in the example scenes in some newer versions of Unity. This could cause exceptions to be thrown in some example scenes.
- Removed some stray uses of the old and deprecated GUIText component. This could cause descriptions for some example scenes not to show up in newer versions of Unity.

## 4.2.10 (2019-11-19)
- Upgrade notes
		- This release is supported in Unity 2017.4 LTS and later. Support for earlier versions has been dropped (however it probably still works in all 2017.x releases).
		- The Unity WebPlayer target is no longer supported. This target was deprecated in Unity 5.4.
- Known bugs
		- In some versions of Unity the spider bot in the example scenes may be missing its animations. This is due to a bug in how Unity upgrades scenes and is unfortunately tricky to fix for all Unity versions simultaneously. This does not affect any other part of the package.
- Fixed a crash when scanning a graph on the WebGL platform and exception support is disabled.

## 4.2.9 (2019-11-15)
- Upgrade notes
		- This release is supported in Unity 2017.4 LTS and later. Support for earlier versions has been dropped (however it probably still works in all 2017.x releases).
		- The Unity WebPlayer target is no longer supported. This target was deprecated in Unity 5.4.
    - Added a visualization for which dimension of the hexagons that is being edited when using a hexagonal grid graph.
        \shadowimage{gridgraph/hexagon_dimension.png}
- RichAI's Funnel Simplification now does a straight line check as the first thing it does.
		This can help improve both performance and the quality of the path.
- Added a small helper window to the scene view when the A* Inspector is open.
	    I think this should work well with the dark Unity theme as well, but please start a thread in the support forum if something looks off.
	    \shadowimage{changelog/scene_view_helper.png}
- Using managed code stripping even up to the High level is now supported out of the box.
- If an RVOController was attached to a GameObject with an AIPath/RichAI component during runtime after the movement script had been initialized then the movement script would previously possibly not find it.
		This is fixed now and the RVOController notifies the AIPath/RichAI script that it has been attached.
- The \link Pathfinding.IAstarAI.SetPath SetPath\endlink method on all built-in movement scripts can now be passed a null parameter. This will clear the current path of the agent.
		In earlier versions this caused an exception to be thrown.
- Fixed NavmeshBase.Linecast (used by RecastGraph and Navmesh linecast methods) would not fill the \a trace out parameter with the starting node in case the start point of the linecast was identical to the end point.
- AIPath and RichAI now clear their paths when they are disabled. Not clearing the path has caused some issues when using object pooling and also some other unexpected behavior.
		If you want to just temporarily disable the movement then use the \link Pathfinding.IAstarAI.canMove canMove\endlink or \link Pathfinding.IAstarAI.isStopped isStopped\endlink properties.
- Fixed RichAI.remainingDistance could refer to the previous path for one frame after a new path had been calculated.
- Fixed AIPath and RichAI scripts sometimes starting with a height of 0.01 when creating a new component in the unity inspector.
		Now they start with a more reasonable height of 2.0.
- The AIBase.usingGravity setter is now protected instead of private which helps when overriding some methods.
- The "Show Surface" mode on recast and navmesh graphs is now enabled by default when creating a new graph.
- The system no longer destroys itself in OnApplicationQuit and is instead always destroyed during OnDestroy. Using OnApplicationQuit could cause trouble when the quitting process was cancelled (e.g. using Application.wantsToQuit).
- Fixed exceptions could be thrown in the editor if the project contains some assemblies that can for some reason not be read. Thanks joshcamas for reporting this.
- Changed the automatic graph coloring limits code to ignore nodes that are unwalkable. This improves the contrast when some unwalkable nodes, that are not visible anyway, have very high penalties (or whatever other value you are visualizing in the scene view).
- Fixed missing null checks in TriangleMeshNode.GetPortal and TriangleMeshNode.SharedEdge.
- The RichAI inspector will now show a helpful warning if one tries to use it in a scene that does not contain a navmesh or recast graph.
- #Pathfinding.GraphUtilities.GetContours for grid graphs now simplifies the contour more consistently. Previously there could be one or two additional points where the algorithm started to traverse the contour.

## 4.2.8 (2019-04-29)
- Made it possible for nearest node queries on point graphs to find the closest connection instead of just the closest node.
		This will make it easier to use graphs when you have many long connections.
		See #Pathfinding.PointGraph.nearestNodeDistanceMode.
- Improved the Seeker->StartEndModifier's Connection snapping mode. Now it will behave better if the path only moves along a single connection in the graph.
- Fixed a crash when deploying for Nintendo Switch due to a Unity bug when setting thread names. Thanks ToastyStoemp for reporting this.
- Fixed some compiler warnings in the ObjImporter class that would show up on some platforms.
- Fixed GridGraph.CalculateConnectionsForCellAndNeighbours would throw an exception when called with the coordinates for a node on the border of the grid. Thanks davidpare for reporting this.

## 4.2.7 (2019-04-05)
- Significantly improved graph rendering performance for recast graphs when using a very large number of small tiles.
- Fixed GridGraph.CountNodes throwing an exception when the graph is not scanned. Now it will return 0.

## 4.2.6 (2019-03-23)
- Fixed AIPath.reachedDestination and RichAI.reachedDestination only worked when the y coordinate of the agent was close to zero... which it of course was in all my tests.
		Sorry about this silly bug and the headache it may have caused.
- Fixed loading a serialized navmesh graph when the source mesh no longer existed would cause the graph to be offset if a navmesh cut was later used to cut it.

## 4.2.5 (2019-02-14)
- Added a new documentation page for how to create and configure graphs during runtime. \ref runtime-graphs.
- Added a new documentation page about editing point graph connections manually. \ref editing-graphs.
- Fixed exceptions could be thrown if the project contains some assemblies that can for some reason not be read.
- Fixed the visualization for unwalkable nodes (red cubes) sometimes disappearing in newer versions of Unity (2018.2+ I think) due to a change in how Time.renderedFrameCount works.
		Thanks Kevin Jenkins for reporting this.
- Fixed applying optimizations (under the Optimizations tab) could cause several error messages to be logged about unsupported platforms in Unity 2018.3 or newer. Thanks NoxMortem for reporting the issue.
- Fixed AIPath throwing an exception if it was given a valid path that contained no nodes at all.
- Made Path.GetTagPenalty public instead of internal.
- Added #Pathfinding.ABPath.FakePath.
- Worked around a null reference exception bug when using IL2CPP and deploying for iPhone.
		This is caused by a bug in the IL2CPP compiler.
- Fixed custom graph types could not be used if they were in another assembly. Thanks juskelis for reporting this and founderio for finding a fix.

## 4.2.4 (2018-12-03)
- Added an option for which dimension of the hexagon to adjust in the grid graph editor when using the hexagonal mode.
		This significantly helps with making a hexagonal graph line up with your other game elements as previously you might have had to manually calculate some complicated conversion factors in order to do this.
- Fixed loading navmesh graphs from a file could be extremely slow if the graph had been saved with the source mesh field set to a mesh with an empty name and your project had a lot of things in its Resources folder.
- Fixed a massive performance regression when using RVO together with IL2CPP and .net 4.6 due to changes in how the .net framework handles locking internally.
- Made GraphNode.Destroy public again (it was made internal in 4.2) because without that, it is not possible to make custom graph types.
- Made Path.PipelineState, Path.duration and Path.pathID public again (they were made internal in 4.2) because those properties are actually useful even for non-internal use.
		This also fixes some incompatibility issues with the Node Canvas integration package. Thanks jsaracev and Grofit for reporting this.

## 4.2.3 (2018-11-07)
- Fixed some compiler warnings in the free version on newer versions of Unity.
- Fixed a bug which caused point graphs to interpret the nearest node distance limit as being 1/1000th the actual value in the free version of the package and in the pro version when not using the 'optimize for sparse graph' option.
		This bug caused the point graph example scene to not work in the free version of the package.

## 4.2.2 (2018-10-25)
- Fixed upgrading from an earlier 4.x version to 4.2 could cause compiler errors in some newer versions of Unity because the UnityPackage doesn't import the new directory structure correctly.

## 4.2.1 (2018-10-23)
- Fixed a bug which caused scanning navmesh graphs to throw an exception when using the free version of the package. Thanks Hunted for reporting the bug.

## 4.2 (2018-10-17)
- 4.1.17 through 4.1.26 were beta versions, all their changelogs have been merged into this one.
- Upgrade notes
		- This release contains some breaking changes (primarily the pivot point change for characters that you can read about below).
			This will affect very few users, but I still recommend that you take a backup of your project before upgrading.
		- Since 4.1.23 the base of the character when using AIPath/RichAI is always at the pivot point of the Transform.
			This reduces code complexity and improves performance. Most users would already have had configured their characters that way, but in
			rare cases you may have to adjust your characters a bit (mostly just move them up or down a bit).
		- When changing some low level fields that can affect the connectivity of the graph such as #Pathfinding.MeshNode.connections you must now call #Pathfinding.GraphNode.SetConnectivityDirty().
			The documentation for all the relevant fields mention this. Currently it only applies to the connection fields on the MeshNode, PointNode and GridNodeBase classes.
			All high level methods that modify the connectivity do this automatically. Most likely you do not have to change your code.
		- When updating the graph using a work item, it is no longer necessary to call the QueueFloodFill or FloodFill methods. Due to the hierarchical graph update mentioned below, this is all handled transparently behind the scenes.
			You should remove any calls to those deprecated methods as they will (for backwards compatibility reasons) force a full recalculation of the connected components which is much slower than what is likely necessary.
- Improvements
		- Converted all comments to XML comments.
			This will drastically improve intellisense for most users as Doxygen style comments are not supported by that many editors.
			As the whole codebase has been converted using a script, there is a potential for errors in the translation.
			Please let me know if you find anything which looks odd or where the intellisense info could be improved.
		- Improved recast graph scanning speed by up to 60% in the unity editor on Mono2x.
			Surisingly this optimization doesn't seem to matter much for other compiler backends.
			This may cause minor changes to your navmesh. If this change causes bad navmeshes to be generated or if it reduces performance, please start a thread in the forum.
		- The RVOController and AIPath/RichAI scripts now take the scale of the object into account when using their respective height and radius fields.
			If you have an existing character with a non-unit scale the height and radius fields will automatically be updated to compensate for this scale.
			However if you are scaling characters during runtime, this may change their behaviour.
		- Added a new \link Pathfinding.GraphMask GraphMask\endlink struct to represent a set of graphs.
			Previously graph masks have been represented using pure integers (e.g. in #Pathfinding.Seeker.graphMask and #Pathfinding.NNConstraint.graphMask).
			The new struct contains some nice helper methods like #Pathfinding.GraphMask.FromGraphName which has been requested by many users.
		- Added a new documentation page describing the inspector: \ref inspector.
		- Added a new documentation page with an overview of the different included movement scripts: \ref movementscripts.
		- Added a new property on all movement scripts called \link Pathfinding.IAstarAI.reachedDestination reachedDestination\endlink.
			The existing \link Pathfinding.IAstarAI.reachedEndOfPath reachedEndOfPath\endlink property has some quirks, which are very reasonable when considering how everything works,
			but are not very intuitive for new users and it can easily lead to quite brittle code. This new property will work as expected for most use cases.
		- Added a height and radius for the AIPath and RichAI movement scripts.
			This in turn deprecates the old Pathfinding.AIBase.centerOffset field as it is now implicitly height/2.
			If an RVOController is attached to the same GameObject, its height and radius will be driven by the movement script's values.
			The script will try to autodetect reasonable height and radius values from other attached components upon upgrading.
		- Improved look of all movement script inspectors by grouping the fields into different sections. Also added some validation to prevent invalid field values.
		- Added #Pathfinding.AstarData.GetNodes.
		- Optimized graph rendering a bit. In particular if multiple scene views/in game cameras are used.
		- The default graph rendering mode is now 'Solid Color' instead of 'Areas'. This new mode will render the graphs with a single color as the name implies.
			The Area coloring mode has turned out to sometimes be confusing for new users and it is not useful that often anyway.
		- Improved performance of small graph updates on large graphs by a very large amount.
			Previously when making a small update to a large graph, updating the connected components of the graph using a flood fill has been the thing which took the longest time by far.
			Using a new internal hierarchical graph it is now possible to update large graphs much faster. For example making a small update to a 1024*1024 grid graph is on the order of 30 times faster and is now perfectly reasonable to do in real time (slightly depending on how the graph looks and its settings).
			The cost of the flood fill was previously offloaded to a separate thread, so it would not always be noticed in the Unity profiler, but it was there and <a href="http://forum.arongranberg.com/t/updategraphs-lag-spikes">could affect the performance of the game in strange ways</a>.
			The performance of scanning a graph or updating the whole graph remains roughly the same.
			For more information about how this works, see #Pathfinding.HierarchicalGraph.
		- Removed small allocation that was previously done for each calculated path on grid graphs.
		- Pathfinding threads now show up in the Unity Profiler in the Timeline view when using a recent Unity version (2017.3 or higher).
		- Scanning layered grid graphs is now approximately 2.2x as fast.
		- Updating layered grid graphs is now approximately 1.5x as fast.
		- Scanning and updating layered grid graphs now allocates a lot less memory.
		- Added assembly definition (.asmdef) files to the package and restructured things a bit.
			This will help cut down on the compilation times in your project. See https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html for more info.
		- Improved performance of scanning and updating recast graphs, in particular with very large tiles.
		- Improved performance of get nearest queries on point graphs when not using \link Pathfinding.PointGraph.optimizeForSparseGraph optimizeForSparseGraph\endlink.
		- Reduced memory usage of local avoidance slightly.
		- All navmesh/recast graphs now support navmesh cutting out of the box and the TileHandlerHelper component is no longer necessary.
			If you have a TileHandlerHelper component in your scene you can safely delete it after upgrading.
		- AIPath/RichAI now has an option for disabling rotation completely. This was possible before as well, but the options you had to set were not entirely intuitive.
			The inspector has been restructured a bit to improve the ease of use.
- Changes
		- Removed non-generic version of PathPool which has been deprecated since early 2016.
		- Removed various methods in the AstarMath and Polygon classes which have been deprecated since early 2016.
		- Removed a few other things that have been deprecated since early 2016.
		- Removed AstarPath.ScanLoop which has been deprecated since 2015.
		- Removed IntRect.Rotate and IntRect.Offset because they were not used by any part of the package.
		- Removed the mostly internal method GraphCollision.Raycast because it was not used by any code in the package.
		- Changed signature and behavior slightly of the mostly internal method \reflink{GraphCollision.CheckHeightAll}.
		- Replaced LayerGridGraph.SampleCell with the new method LayerGridGraph.SampleHeights which does essentially the same thing, but in a more efficient way.
		- The rotationIn2D option for the built-in movement scripts has been renamed to \reflink{AIBase.orientation} and is now an enum.
- Fixes
		- Replaced usage of the WWW class with the newer UnityWebRequest in the update checker to avoid a deprecation compiler warning in newer versions of Unity.
		- Fixed a bug in the A* inspector that could cause the inspector to log errors and look weird in Unity 2018.3b5 and up. Thanks Aisit for reporting the bug.
		- Fixed a rare race condition that could cause exceptions in various parts of the code due to the StackPool class not being thread safe. Thanks nindim for reporting the issue.
		- Fixed applying optimizations (under the Optimizations tab) could cause several error messages to be logged about unsupported platforms in Unity 2018.2 or newer and in 2017.4.
		- Fixed trying to set a tag on a node to a value outside the valid range (0...32) could silently cause very weird behavior as other fields were affected by that.
		- Fixed minor allocation when calling #AstarPath.GetNearest without an NNConstraint.
		- Fixed a bug introduced in 4.1.22 which could cause a null reference exception to sometimes be thrown when visualizing graphs in the scene view.
		- Fixed #Pathfinding.Path.IsDone could due to a race condition return true a tiny amount of time before the path was actually calculated.
		- Fixed a bug introduced in 4.1.18 which caused various rotation settings to be hidden in the inspector for the RichAI component.
		- Fixed removing a movement script from the same component as an RVOController during runtime would not cause the RVOController to stop using the (now destroyed) movement script's position for local avoidance calculations.
		- Fixed the ObjectPlacer example script which is used in some example scenes would not always update the graph properly after destroying objects.
			In Unity object destruction is delayed until after the Update loop, but the script did not wait to update the graph until after the object had actually been destroyed. Thanks djzombie for reporting this.
		- Fixed recalculating a part of a recast graph using a graph update object would not expand the bounding box with the character radius to make sure that all tiles that could be affected by something inside the box were updated.
		- Worked around occational crashes when using RVO with the IL2CPP backend due to a bug in IL2CPP.
		- Fixed scanning a recast graph could throw an exception if there was a RecastNavmeshModifier component attached to a GameObject with a MeshFilter that had a null/missing mesh. Thanks ccm for finding the bug.
		- Fixed FloodPath throwing an exception when starting on a node that isn't connected to any other node. Thanks BulwarkStudios for finding the bug.
		- Fixed applying optimizations (under the Optimizations tab) could cause several error messages to be logged about unsupported platforms in Unity 2018.1 or newer. Thanks NFMonster for reporting the issue.
		- Rotation speed and acceleration are now decoupled for AIPath and RichAI. Previously the acceleration limited how quickly the agents could rotate due to how the math for <a href="https://en.wikipedia.org/wiki/Centripetal_force">centripetal acceleration</a> works out.
			This was originally added in 4.1.11, but due to a typo the change was disabled. Now it is properly working.
		- Fixed GraphModifier.OnGraphsPostUpdate being called multiple times during a single update if multiple tiles were updated due to navmesh cutting.
			This can significantly improve performance if you are using many off-mesh links or you are using the RVONavmesh component.
		- Fixed an edge case bug which could cause get nearest node queries on a newly scanned point graph to return older destroyed nodes.
		- Fixed work items throwing exceptions could cause some internal data to get into an invalid state.
		- Fixed tree colliders would not be rasterized correctly when using the recast graph and the tree prefabs had been scaled.
		- Fixed not finding graph types and graph editors from other assemblies than the one which contains the pathfinding system (important when using asmdef files).
		- Fixed #AstarPath.FlushWorkItems always pausing the pathfinding threads even if no work items were queued.
		- Fixed loading navmesh graphs (and potentially in rare cases: recast graphs) from the cache or from a file could result in bad graph data which confused the funnel modifier which would create very zig-zaggy paths.
			If you have been experiencing this then just re-save your graphs/regenerate the cache after you have updated and then it should work as it should. Thanks Igor Aherne for reporting this.

## 4.1.16 (2018-04-26)
- Fixed PointNode.ContainsConnection could throw an exception if the node didn't have any connections.
- Fixed AILerp's started out with a destination set to (0,0,0) instead of not having a destination set.
		So if you did not set a destination for it, it would try to move to the world origin.

## 4.1.15 (2018-04-06)
- Fixed RichAI.desiredVelocity always being zero. Thanks sukrit1234 for finding the bug.
- Added some video examples to \link Pathfinding.AIPath.pickNextWaypointDist AIPath.pickNextWaypointDist\endlink.
- Fixed a bug introduced in 4.1.14 which caused scanning recast graphs in the Unity editor to fail with an error sometimes.
- Fixed the position returned from querying the closest point on the graph to a point (AstarPath.GetNearest) on layered grid graphs would always be the node center, not the closest point on the node's surface. Thanks Kevin_Jenkins for reporting this.
		This caused among other things the ClosestOnNode option for the Seeker's StartEndModifier to be identical to the SnapToNode option.
- Fixed RVOController.velocity being zero when the game was paused (Time.timeScale = 0).

## 4.1.14 (2018-03-06)
- Fixed Pathfinding.GridNode.ClosestPointOnNode being completely broken. Thanks Ivan for reporting this.
		This was used internally in some cases when pathfinding on grid graphs. So this fixes a few cases of strange pathfinding results too.
- It is now possible to use pathfinding from editor scripts. See \ref editor-mode.

## 4.1.13 (2018-03-06)
- Fixed LayerGridGraph.GetNode not performing out of bounds checks.
- Exposed a public method \link Pathfinding.PointGraph.ConnectNodes PointGraph.ConnectNodes\endlink which can be useful if you are creating a graph from scratch using e.g PointGraph.AddNode.
- Improved the \ref multiple-agent-types tutorial.
- Improved the \ref custom_movement_script tutorial, among other things it can now also be followed if you are creating a 2D game.
		The movement script that you write has also been improved.
- Improved how the RichAI movement script keeps track of the node it is on. It should now be more stable in some cases, especially when the ground's y-coordinate lines up badly with the y-coordinate of the navmesh.
- Added an \link Pathfinding.AIPath.constrainInsideGraph option\endlink to AIPath for constraining the agent to be inside the traversable surface of the graph at all times.
		I think it should work everywhere without any issues, but please post in the forum if anything seems to break.
- Fixed the proper fonts were not imported in the documentation html, so for many browsers it fell back to some other less pretty font.

## 4.1.12 (2018-02-27)
- Fixed right clicking on array elements in the Unity inspector would bring up the 'Show in online documentation' context menu instead of the Unity built-in context menu (which is very useful).
- Navmesh assets used in the navmesh graph no longer have to be at the root of the Resources folder, they can be in any subfolder to the Resources folder.

## 4.1.11 (2018-02-22)
- You can now set which graphs an agent should use directly on the Seeker component instead of having to do it through code.
		\shadowimage{multiple_agents/seeker.png}
- Added tutorial for how to deal with agents of different sizes: \ref multiple-agent-types.
- Fixed scanning recast graphs could in rare cases throw an exception due to a multithreading race condition. Thanks emrys90 for reporting the bug.
- Fixed a regression in 4.0.6 which caused position based penalty to stop working for layered grid graphs. Thanks DougW for reporting the bug.
- Rotation speed and acceleration are now decoupled for AIPath and RichAI. Previously the acceleration limited how quickly the agents could rotate due to how the math for <a href="https://en.wikipedia.org/wiki/Centripetal_force">centripetal acceleration</a> works out.
- Acceleration can now be set to a custom value on the AIPath class. It defaults to a 'Default' mode which calculates an acceleration such that the agent reaches its top speed in about 0.4 seconds. This is the same behaviour that was hardcoded in earlier versions.
- Fixed a bug in \link Pathfinding.GraphUtilities.GetContours GraphUtilities.GetContours\endlink for grid graphs when the nodes parameter was explicitly passed as non null that could cause some contours not to be generated. Thanks andrewBeers for reporting the bug.
- Improved documentation for \link Pathfinding.StartEndModifier.Exactness StartEndModifier.Exactness\endlink.

## 4.1.10 (2018-01-21)
- 4.1.0 through 4.1.9 were beta versions, all their changelogs have been merged into this one.
- Upgrade notes
		- Fixed the AIPath script with rotationIn2D would rotate so that the Z axis pointed in the -Z direction instead of as is common for Unity 2D objects: to point in the +Z direction.
		- ALL classes are now inside the Pathfinding namespace to reduce potential naming collisions with other packages.
			Make sure you have "using Pathfinding;" at the top of your scripts.
			Previously most scripts have been inside the Pathfinding namespace, but not all of them.
			The exception is the AstarPath script to avoid breaking too much existing code (and it has a very distinctive name so name collisions are not likely).
		- Since the API for several movement scripts have been unified (see below), many members of the movement scripts have been deprecated.
			Your code should continue to work exactly as before (except bugs of course, but if some other behaviour is broken, please start a thread in the forum) but you may get deprecation warnings.
			In most cases the changes should be very easy to make as the visible changes mostly consist of renames.
		- A method called \link Pathfinding.IAstarAI.SetPath SetPath\endlink has been added to all movement scripts. This replaces some hacks you could achieve by calling the OnPathComplete method on the movement scripts
			from other scripts. If you have been doing that you should now call SetPath instead.
		- Paths calculated with a heuristic scale greater than 1 (the default is 1) might be slightly less optimal compared to before.
			See below for more information.
		- The StartEndModifier's raycasting options are now only used if the 'Original' snapping option is used as that's the only one it makes sense for.
		- The RaycastModifier has changed a bit, so your paths might look slightly different, however in all but very rare cases it should be at least as good as in previous versions.
		- Linecast methods will now assign the #Pathfinding.GraphHitInfo.node field with the last node that was traversed in case no obstacle was hit, previously it was always null.
		- Multithreading is now enabled by default (1 thread). This may affect you if you have been adding the AstarPath component during runtime using a script, though the change is most likely positive.
		- The DynamicObstacle component will now properly update the graph when the object is deactivated since the object just disappeared and shouldn't block the graph anymore.
			Previously it only did this if the object was destroyed, not if it was deactivated.
		- If you have written a custom graph type you may have to change the access modifier on some methods.
			For example the ScanInternal method has been changed from being public to being protected.
		- Some internal methods on graphs have been hidden. They should never have been used by user code
			but in case you have done that anyway you will have to access them using the IGraphInternals or IUpdatableGraph interface now.
		- Removed some compatibility code for Seekers for when upgrading from version 3.6.7 and earlier (released about 2 years ago).
			If you are upgrading from a version that old then the 'Valid Tags' field on the Seeker component may get reset to the default value.
			If you did not use that field then you will not have to do anything.
		- AIPath now rotates towards actual movement direction when RVO is used.
- Improvements
		- Improved pathfinding performance by around 8% for grid graphs, possibly more for other graph types.
			This involved removing a special case for when the pathfinding heuristic is not <a href="https://en.wikipedia.org/wiki/Admissible_heuristic">admissable</a> (in short, when A* Inspector -> Settings -> Heuristic Scale was greater than 1).
			Now paths calculated with the heuristic scale greater than 1 might be slightly less optimal compared to before.
			If this is important I suggest you reduce the heuristic scale to compensate.
			Note that as before: a heuristic scale of 1 is the default and if it is greater than 1 then the calculated paths may no longer be the shortest possible ones.
		- Improved overall pathfinding performance by an additional 10-12% by heavily optimizing some core algorithms.
		- Improved performance of querying for the closest node to a point when using the PointGraph and \link Pathfinding.PointGraph.optimizeForSparseGraph optimizeForSparseGraph\endlink.
			The improvements are around 7%.
		- Unified the API for the included movement scripts (AIPath, RichAI, AILerp) and added a large number of nice properties and functionality.
			- The \link Pathfinding.IAstarAI IAstarAI\endlink interface can now be used with all movement scripts.
			- To make it easier to migrate from Unity's navmesh system, this interface has been designed to be similar to Unity's NavmeshAgent API.
			- The interface has several nice properties like:
				\link Pathfinding.IAstarAI.remainingDistance remainingDistance\endlink,
				\link Pathfinding.IAstarAI.reachedEndOfPath reachedEndOfPath\endlink,
				\link Pathfinding.IAstarAI.pathPending pathPending\endlink,
				\link Pathfinding.IAstarAI.steeringTarget steeringTarget\endlink,
				\link Pathfinding.IAstarAI.isStopped isStopped\endlink,
				\link Pathfinding.IAstarAI.destination destination\endlink, and many more.
			- You no longer need to set the destination of an agent using a Transform object, instead you can simply set the \link Pathfinding.IAstarAI.destination destination\endlink property.
				Note that when you upgrade, a new AIDestinationSetter component will be automatically created which has a 'target' field. So your existing code will continue to work.
		- Improved behavior when AIPath/RichAI characters move down slopes.
			Previously the way gravity was handled could sometimes lead to a 'bouncing' behavior unless the gravity was very high. Old behavior on the left, new behavior on the right.
			\htmlonly <video class="tinyshadow" controls="true" loop="true"><source src="images/changelog/ai_slope.mp4" type="video/mp4" /></video> \endhtmlonly
		- Improved the grid graph inspector by adding preconfigured modes for different node shapes: square grids, isometric grids and hexagons.
			This also reduces clutter in the inspector since irrelevant options can be hidden.
			\shadowimage{changelog/grid_shape.png}
		- For 2D grid graphs the inspector will now show a single rotation value instead of a full 3D rotation which makes it a lot easier to configure.
		- Improved the performance of the \link Pathfinding.RaycastModifier RaycastModifier\endlink significantly. Common speedups on grid graphs range from 2x to 10x.
		- The RaycastModifier now has a \link Pathfinding.RaycastModifier.Quality quality enum\endlink. The higher quality options use a new algorithm that is about the same performance (or slightly slower) compared to the RaycastModifier in previous versions
			however it often manages to simplify the path a lot more.
			The quality of the previous RaycastModifier with default settings corresponds to somewhere between the Low and Medium qualities.
		- Improved support for HiDPI (retina) screens as well as improved visual coherency for some icons.
			\shadowimage{changelog/retina_icons.png}
		- Improved the 'eye' icon for when a graph's gizmos are disabled to make it easier to spot.
		- Added \link Pathfinding.GridGraph.CalculateConnectionsForCellAndNeighbours GridGraph.CalculateConnectionsForCellAndNeighbours\endlink.
		- AIPath now works with point graphs in 2D as well (assuming the 'rotate in 2D' checkbox is enabled).
		- Improved the performance of the RVONavmesh component when used together with navmesh cutting, especially when many navmesh cuts are moving at the same time.
		- A warning is now displayed in the editor if one tries to use both the AIDestinationSetter and Patrol components on an agent at the same time.
		- Improved linecasts on recast/navmesh graphs. They are now more accurate (there were some edge cases that previously could cause it to fail) and faster.
			Performance has been improved by by around 3x for longer linecasts and 1.4x for shorter ones.
		- Linecast methods will now assign the #Pathfinding.GraphHitInfo.node field with the last node that was traversed in case no obstacle was hit.
		- Linecast on graphs now set the hit point to the endpoint of the line if no obstacle was hit. Previously the endpoint would be set to Vector3.zero. Thanks borluse for suggesting this.
		- Multithreading is now enabled by default (1 thread).
		- The DynamicObstacle component now works with 2D colliders.
		- Clicking on the graph name in the inspector will no longer focus the name text field.
			To edit the graph name you will now have to click the Edit/Pen button to the right of the graph name.
			Previously it was easy to focus the text field by mistake when you actually wanted to show the graph settings.
			\shadowimage{changelog/edit_icon.png}
		- Reduced memory usage of the PointGraph when using \link Pathfinding.PointGraph.optimizeForSparseGraph optimizeForSparseGraph\endlink.
		- Improved the StartEndModifier inspector slightly.
		- The Seeker inspector now has support for multi-editing.
		- The AIPath and RichAI scripts now rotate to face the direction they are actually moving with when using local avoidance (RVO)
			instead of always facing the direction they want to move with. At very low speeds they fall back to looking the direction they want to move with to avoid jitter.
		- Improved the Seeker inspector. Unified the UI for setting tag penalties and determining if a tag should be traversable.
			\shadowimage{changelog/seeker_tags.png}
		- Reduced string allocations for error messages when paths fail.
		- Added support for 2D physics to the #Pathfinding.RaycastModifier component.
		- Improved performance of GraphUpdateObjects with updatePhysics=false on rotated navmesh/recast graphs.
		- Improved the inspector for AILerp.
		- RVO obstacles can now be visualized by enabling the 'Draw Obstacles' checkbox on the RVOSimulator component.
			\shadowimage{changelog/rvo_navmesh_obstacle.png}
		- Reduced allocations in the funnel modifier.
		- Added a 'filter' parameter to \link Pathfinding.PathUtilities.BFS PathUtilities.BFS\endlink and \link Pathfinding.PathUtilities.GetReachableNodes PathUtilities.GetReachableNodes\endlink.
		- Added a method called \link Pathfinding.IAstarAI.SetPath SetPath\endlink to all movement scripts.
		- Added \link Pathfinding.GraphNode.Graph GraphNode.Graph\endlink.
		- Added #Pathfinding.MeshNode.ContainsPoint(Vector3) in addition to the already existing MeshNode.ContainsPoint(Int3).
		- Added #Pathfinding.MeshNode.ContainsPointInGraphSpace.
		- Added #Pathfinding.TriangleMeshNode.GetVerticesInGraphSpace.
		- Added Pathfinding.AstarData.FindGraph(predicate).
		- Added Pathfinding.AstarData.FindGraphWhichInheritsFrom(type).
		- Added a new class \link Pathfinding.GraphUtilities GraphUtilities\endlink which has some utilities for extracting contours of graphs.
		- Added a new method Linecast(GridNodeBase,GridNodeBase) to the GridGraph class which is much faster than the normal Linecast methods.
		- Added \link Pathfinding.GridGraph.GetNode(int,int) GridGraph.GetNode(int,int)\endlink.
		- Added MeshNode.AddConnection(node,cost,edge) in addition to the already existing AddConnection(node,cost) method.
		- Added a \reflink{Pathfinding.NavMeshGraph.recalculateNormals} setting to the navmesh graph for using the original mesh normals. This is useful for spherical/curved worlds.
- Documentation
		- Added a documentation page on error messages: \ref error-messages.
		- Added a tutorial on how to create a wandering AI: \ref wander.
		- Added tutorial on bitmasks: \ref bitmasks.
		- You can now right-click on most fields in the Unity Inspector to bring up a link to the online documentation.
			\shadowimage{inspector_doc_links.png}
		- Various other documentation improvements and fixes.
- Changes
		- Height or collision testing for grid graphs now never hits triggers, regardless of the Unity Physics setting 'Queries Hit Triggers'
		which has previously controlled this.
		- Seeker.StartPath will no longer overwrite the path's graphMask unless it was explicitly passed as a parameter to the StartPath method.
		- The built in movement scripts no longer uses a coroutine for scheduling path recalculations.
			This shouldn't have any impact for you unless you have been modifying those scripts.
		- Replaced the MineBotAI script that has been used in the tutorials with MineBotAnimation.
			The new script does not inherit from AIPath so in the example scenes there is now one AIPath component and one MineBotAnimation script on each unit.
		- Removed prompt to make the package support UnityScript which would show up the first time you used the package in a new project.
			Few people use UnityScript nowadays so that prompt was mostly annoying. UnityScript support can still be enabled..
		- If deserialization fails, the graph data will no longer be stored in a backup byte array to be able to be recovered later.
			This was not very useful, but more importantly if the graph data was very large (several megabytes) then Unity's Undo system would choke on it
			and essentially freeze the Unity editor.
		- The StartEndModifier's raycasting options are now only used if the 'Original' snapping option is used as that's the only one it makes sense for.
		- The RaycastModifier.subdivideEveryIter field has been removed, this is now always enabled except for the lowest quality setting.
		- The RaycastModifier.iterations field has been removed. The number of iterations is now controlled by the quality field.
			Unfortunately this setting cannot be directly mapped to a quality value, so if you are upgrading all RaycastModifier components will use the quality Medium after the upgrade.
		- The default value for \reflink{RVOController.lockWhenNotMoving} is now false.
		- Tiles are now enabled by default on recast graphs.
		- Modifiers now register/unregister themselves with the Seeker component during OnEnable/OnDisable instead of Awake/OnDestroy.
			If you have written any custom modifiers which defines those methods you may have to add the 'override' modifier to those methods and call base.OnEnable/OnDisable.
		- When paths fail this is now always logged as a warning in the Unity console instead of a normal log message.
		- Node connections now store which edge of the node shape that is used for that node. This is used for navmesh/recast graphs.
		- The \reflink{RVOController.velocity} property can now be assigned to and that has the same effect as calling ForceSetVelocity.
		- Deprecated the RVOController.ForceSetVelocity method. You should use the velocity property instead.
		- All graphs now explicitly implement the IUpdatableGraph interface.
			This is done to hide those methods (which should not be used directly) and thereby reduce confusion about which methods should be used to update graphs.
		- Hid several internal methods behind the IGraphInternals interface to reduce clutter in the documentation and IntelliSense suggestions.
		- Removed NavGraph.UnloadGizmoMeshes because it was not used for anything.
		- Since 4.0 individual graphs can be scanned using AstarPath.Scan. The older NavGraph.Scan method now redirects to that method
			which is more robust. This may cause slight changes in behavior, however the recommendation in the documentation has always been to use AstarPath.Scan anyway
			so I do not expect many to have used the NavGraph.Scan method.
		- Deprecated the NavGraph.ScanGraph method since it just does the same thing as NavGraph.Scan.
		- Deprecated the internal methods Path.LogError and Path.Log.
		- Added the new internal method Path.FailWithError which replaces LogError and Log.
		- Made the AIPath.TrySearchPath method private, it should never have been public to begin with.
- Fixes
		- Fixed AIPath/RichAI throwing exceptions in the Unity Editor when drawing gizmos if the game starts while they are enabled in a disabled gameObject.
		- Fixed some typos in the documentation for PathUtilities.BFS and PathUtilities.GetReachableNodes.
		- For some point graph settings, path requests to points that could not be reached would fail completely instead of going to the closest node that it could reach. Thanks BYELIK for reporting this bug.
			If you for some reason have been relying on the old buggy behavior you can emulate it by setting A* Inspector -> Settings -> Max Nearest Node Distance to a very low value.
		- Fixed connection costs were assumed to be equal in both directions for bidirectional connections.
		- Fixed a compiler error when building for UWP/HoloLens.
		- Fixed some cases where circles used for debugging could have a much lower resolution than intended (Pathfinding.Util.Draw.CircleXZ).
		- Fixed RVO agents which were locked but some script sent it movement commands would cause the RVO system to think it was moving
			even though it was actually stationary, causing some odd behavior. Now locked agents are always treated as stationary.
		- Fixed RVO obstacles generated from graph borders (using the RVONavmesh component) could be incorrect if a tiled recast graph and navmesh cutting was used.
			The bug resulted in an RVO obstacle around the tile that was most recently updated by a navmesh cut even where there should be no obstacle.
		- Fixed the RVONavmesh component could throw an exception in some cases when using tiled recast graphs.
		- Fixed a regression in some 4.0.x version where setting \link Pathfinding.RVOController.velocity RVOController.velocity\endlink to make the agent's movement externally controlled
			would not work properly (the system would always think the agent had a velocity of zero).
		- Fixed the RichAI movement script could sometimes get stuck on the border between two tiles.
			(due to a possibility of division by zero that would cause its velocity to become NaN).
		- Fixed AIPath/RichAI movement not working properly with rigidbodies in Unity 2017.3+ when the new physics setting "Auto Sync Transforms" was disabled. Thanks DougW for reporting this and coming up with a fix.
		- Fixed a few cases where \link Pathfinding.RichAI RichAI\endlink would automatically recalculate its path even though \link Pathfinding.RichAI.canSearch canSearch\endlink was disabled.
		- Fixed some compiler warnings when using Unity 2017.3 or later.
		- Fixed graphical artifacts in the graph visualization line drawing code which could show up at very large coordinate values or steep viewing angles.
			Differential calculus can be really useful sometimes.
		- Fixed the \ref MultiTargetPathExample.cs.
		- Fixed the width/depth fields in the recast graph inspector causing warnings to be logged (introduced in 4.1.7). Thanks NoxMortem for reporting this.
		- Fixed the Pathfinding.GraphHitInfo.tangentOrigin field was offset by half a node when using linecasting on grid graphs.
		- Fixed the AIPath script with rotationIn2D would rotate so that the Z axis pointed in the -Z direction instead of as is common for Unity 2D objects: to point in the +Z direction.
		- Fixed the AILerp script with rotationIn2D would rotate incorrectly if it started out with the Z axis pointed in the -Z direction.
		- Clamp recast graph bounding box size to be non-zero on all axes.
		- The DynamicObstacle component will now properly update the graph when the object is deactivated since the object just disappeared and shouldn't block the graph anymore.
			Previously it only did this if the object was destroyed, not if it was deactivated.
		- Fixed \link Pathfinding.AILerp AILerp\endlink ceasing to work properly if one of the paths it tries to calculate fails.
		- Fixed the \link Pathfinding.FunnelModifier FunnelModifier\endlink could yield a zero length path in some rare circumstances when using custom node links.
			This could lead to an exception in some of the movement scripts. Thanks DougW for reporting the bug.
		- Fixed calling Seeker.CancelCurrentPathRequest could in some cases cause an exception to be thrown due to multithreading race conditions.
		- Fixed a multithreading race condition which could cause a path canceled by Seeker.CancelCurrentPathRequest to not actually be canceled.
		- Fixed a rare ArrayOutOfBoundsException when using the FunnelModifier with the 'unwrap' option enabled.
		- Fixed Seeker -> Start End Modifier could not be expanded in the Unity inspector. Thanks Dee_Lucky for reporting this.
		- Fixed a few compatiblity bugs relating to AIPath/RichAI that were introduced in 4.1.0.
		- Fixed funnel modifier could sometimes fail if the agent started exactly on the border between two nodes.
		- Fixed another bug which could cause the funnel modifier to produce incorrect results (it was checking for colinearity of points in 2D instead of in 3D).
		- Fixed the funnel modifier would sometimes clip a corner near the end of the path.
		- Fixed ProceduralGraphMover would not detect user defined graphs that were subclasses of the GridGraph class. Thanks viveleroi for reporting this.
		- Fixed enabling and disabling a AIPath or RichAI component a very large number of times could potentially have a negative performance impact.
		- Fixed AIPath/RichAI would continue searching for paths even when the component had been disabled.
		- MeshNode.ContainsPoint now supports rotated graphs properly. MeshNode is used in navmesh and recast graphs.
		- Fixed Linecast for navmesh and recast graphs not working for rotated graphs.
		- Fixed RVONavmesh component not working properly with grid graphs that had height differences.
		- Fixed 2D RVO agents sometimes ignoring obstacles.
		- Fixed RVONavmesh not removing the obstacles it had created when the component was disabled.
		- Fixed RaycastModifier could miss obstacles when thick raycasting was used due to Unity's Physics.SphereCast method not
			reporting hits very close to the start of the raycast.
		- In the free version the inspector for RaycastModifier now displays a warning if graph raycasting is enabled since
			for all built-in graphs raycasts are only supported in the pro version.
		- Fixed some cases where the funnel modifier would produce incorrect results.
		- Fixed typo in a private method in the AstarPath class. Renamed the UpdateGraphsInteral method to UpdateGraphsInternal.
		- Fixed AIPath.remainingDistance and AIPath.targetReached could be incorrect for 1 frame when a new path had just been calculated (introduced in a previous beta release).

## 4.0.11 (2017-09-09)
- Fixed paths would ignore the ITraversalProvider (used for the turn based utilities) on the first node of the path, resulting in successful paths where they should have failed.
- Fixed BlockManager.BlockMode.AllExceptSelector could often produce incorrect results. Thanks Cquels for spotting the bug.
- Fixed various bugs related to destroying/adding graphs that could cause exceptions. Thanks DougW for reporting this.
- Fixed destroying a grid graph would not correctly clear all custom connections. Thanks DougW for reporting this.
- Fixed the MultiTargetPath did not reset all fields to their default values when using path pooling.
- Added some additional error validation in the MultiTargetPath class.
- Fixed scanning a recast graph that was not using tiles using Unity 2017.1 or later on Windows could block indefinitely. Thanks David Drummond and ceebeee for reporting this.
- Improved compatibility with Nintendo Switch. Thanks Noogy for the help.
- Fixed GraphUpdateScene would not handle the GameObject's scale properly which could cause it to not update some nodes.
- Fixed a regression in 4.0 which could cause the error to be omitted from log messages when paths failed.
- Fixed several bugs relating to #Pathfinding.NNConstraint.distanceXZ and #Pathfinding.NavmeshBase.nearestSearchOnlyXZ. Thanks koirat for reporting this.
- Fixed scanning a graph that threw an error would prevent any future scans. Thanks Baste for reporting this.
- Added a new get started video tutorial. See \ref getstarted.
- The PointGraph.nodeCount property is now protected instead of private, which fixes some compatibility issues.
- Improved compatibility with Unity 2017.1, esp. when using the experimental .Net 4.6 target. Thanks Scott_Richmond for reporting the issues.
- Fixed DynamicObstacle trying to update the graphs even when outside of play mode.
- Fixed runtime error when targeting the Windows Store. Thanks cedtat for reporting the bug.
- Fixed compilation error when targeting the Windows Store. Introduced in 4.0.3. Thanks cedtat for reporting the bug.

## 4.0.10 (2017-05-01)
- Fixed compiler errors in the free version because the ManualRVOAgent.cs script being included by mistake. Thanks hummerbummer for reporting the issue.
- Fixed Unity's scene view picking being blocked by graph gizmos. Thanks Scott_Richmond for reporting the bug.

## 4.0.9 (2017-04-28)
- Significantly improved performance and reduced allocations when recalculating indivudal recast tiles during runtime and there are terrains in the scene.
- Fixed the GraphUpdateScene inspector showing a warning for one frame after the 'convex' field has been changed.
- Fixed a few compiler warnings in Unity 5.6. Thanks TotalXep for reporting the issue.
- Fixed graph drawing could generate large amounts of garbage due to a missing GetHashCode override which causes Mono to have to allocate some dummy objects.
- Fixed graph gizmo lines could be rendered incorrectly on Unity 5.6 on mac and possibly on Windows too.

## 4.0.8 (2017-04-28)
- Added rotationIn2D to the AIPath script. It makes it possible to use the Y axis as the forward axis of the character which is useful for 2D games.
- Exposed the GridGraph.LayerCount property which works for both grid graphs and layered grid graphs (for grid graphs it always returns 1).
- Made the LayerGridGraph.layerCount field internal to discourage its use outside the LayerGridGraph class.
- Fixed exception when destroying some graph types (introduced in 4.0.6). Thanks unfalco for reporting the bug.
- Fixed exception in GridGraph.GetNodesInRegion when being called with an invalid rectangle or a rectangle or bounds object that was completely outside the graph. Thanks WillG for finding the bug.
- Fixed AIPath/RichAI not rotating to the correct direction if they started in a rotation such that the forward axis was perpendicular to the movement plane.

## 4.0.7 (2017-04-27)
- Fixed 2D example scenes had their grids rotated by (90,0,0) instead of (-90,0,0).
		It doesn't matter for those scenes, but the (-90,0,0) leads to more intuitive axis rotations for most use cases. Thanks GeloMan for noticing this.
- Renamed AISimpleLerp to AILerp in the component menu as the documentation only refers to it by the name 'AILerp'.
- Added a new documentation page and video tutorial (\ref pathfinding-2d) showing how to configure pathfinding in 2D games.

## 4.0.6 (2017-04-21)
- Fixed creating a RichAI and in the same frame setting the target and calling UpdatePath would always result in that path being canceled.
- Fixed a race condition which meant that if you called RichAI.UpdatePath, AILerp.SearchPath or AIPath.SearchPath during the same frame that the agent was created
		then the callback for that path would sometimes be missed and the AI would wait indefinitely for it. This could cause the agents to sometimes never start moving.
- Fixed adding a new graph while graph updates were running at the same time could potentially cause errors.
- Added NavGraph.exists which will become false when a graph has been destroyed.
- Fixed TileHandlerHelper could throw exceptions if the graph it was tracking was destroyed.
- Fixed TileHandlerHelper not detecting new NavmeshCut or NavmeshAdd components that were created before the
		TileHandlerHelper component was created or when it was disabled.
- TileHandlerHelper no longer logs an error if it is created before a recast/navmesh graph exists in the scene
		and when one is created the TileHandlerHelper will automatically detect it and start to update it.
- Fixed TileHandlerHelper could throw exceptions if the graph it was tracking changed dimensions.
- Fixed recast graphs would always rasterize capsule colliders as if they had their 'direction' setting set to 'Y-axis'. Thanks emrys90 for reporting the bug.
- The package now contains a 'documentation.html' file which contains an offline version of the 'Get Started' tutorial.

## 4.0.5 (2017-04-18)
- Improved compatibility with Opsive's Behavior Designer - Movement Pack (https://www.assetstore.unity3d.com/en/#!/content/16853).
		- The 4.0 update combined with the Movement Pack caused some compiler errors previously.

## 4.0.4 (2017-04-17)
- Fixed the funnel modifier not working if 'Add Points' on the Seeker's Start End Modifier was enabled. Thanks Blaze_Barclay for reporting it.
- Fixed code typo in the \ref write-modifiers tutorial as well as made a few smaller improvements to it.
- Fixed some cases where the LegacyRVOController would not behave like the RVOController before version 4.0.
- Fixed LegacyAIPath not using the same custom inspector as the AIPath component.

## 4.0.3 (2017-04-16)
- Improved code style and improved documentation for some classes.
- Reduced memory allocations a bit when using the NavmeshAdd component.
- Fixed graph types not necessarily being initialized when scanning the graph outside of play mode.
- Fixed LayerGridGraph not reporting scanning progress properly.
		This caused it to not work well with ScanAsync and when scanning the graph in the editor the progress bar would only update once the whole graph had been scanned.
- Removed the DebugUtility class which was only used for development when debugging the recast graph.

## 4.0.2 (2017-04-16)
- Fixed a minor bug in the update checker.
- Deduplicated code for drawing circles and other shapes using Debug.Draw* or Gizmos.Draw* and moved this code to a new class Pathfinding.Util.Draw.

## 4.0.1 (2017-04-15)
- Improved how AIPath and RichAI work with rigidbodies.
- Added option for gravity to AIPath.
- Removed the RichAI.raycastingForGroundPlacement field as it is automatically enabled now if any gravity is used.
- AIPath and RichAI now inherit from the same base class Pathfinding.AIBase.

## 4.0 (2017-04-10)
- Upgrade Notes
		- This release contains some significant changes. <b>It is strongly recommended that you back up your
			project before upgrading</b>.
		- If you get errors immediately after upgrading, try to delete the AstarPathfindingProject folder
			and import the package again. Sometimes UnityPackages will leave old files which can cause issues.
		- Moved some things to inside the Pathfinding namespace to avoid naming collisions with other packages.
			Make sure you have the line 'using Pathfinding;' at the top of your scripts.
			Some example scripts have been moved to the Pathfinding.Examples namespace.
		- The RVOController component no longer handles movement as it turned out that was a bad idea.
			Having multiple components that handled movement (e.g RichAI and RVOController) didn't turn out well
			and it was very hard to configure the settings so that it worked well.
			The RVOController now exposes the CalculateMovementDelta method which allows other scripts to
			ask it how the local avoidance system thinks the character should move during this frame.
			If you use the RichAI or AIPath components for movement, everything should work straight away.
			If you use a custom movement script you may need to change your code to use the CalculateMovementDelta
			method for movement. Some settings may need to be tweaked, but hopefully it should not be too hard.
		- Node connections are now represented using an array of structs (of type \link Pathfinding.Connection Connection\endlink) instead of
			one array for target nodes and one array for costs.
		- When upgrading an existing project legacy versions of the RVOController, RichAI, AIPath and GraphUpdateScene components
			will be used for compatibility reasons. You will have to click a button in the inspector to upgrade them to the latest versions.
			I have tried to make sure that the movement scripts behave the same as they did before version 4.0, but it is possible that there are some minor differences.
			If you have used a custom movement script which inherits from AIPath or RichAI then the legacy components cannot be used automatically, instead the new versions will be used from the start.
- New Features And Improvements
		- Local Avoidance
			- The RVO system has been cleaned up a lot.
				- Agents will now always avoid walls and obstacles even if that would put them on a collision course with another agent.
					This helps with a previous problem of agents being able to be pushed into walls and obstacles (note that RVONavmesh or RVOSquareObstacle still need to be used).
				- The RVOSimulator can now be configured for XZ space or XY space (2D).
				- The RVOController no longer handles movement itself as this turned out to be a really bad idea (see upgrade notes section).
				- The RVOController can now be used to stop at a target much more precisely than before using the SetTarget method.
				- Agents are now \link Pathfinding.RVO.RVOSimulator.symmetryBreakingBias biased slightly\endlink towards passing other agents on the right side, this helps resolve some situations
					with a lot of symmetry much faster.
				- All fuzzy and hard to adjust parameters from the \link Pathfinding.RVO.RVOSimulator RVOSimulator\endlink component have been removed.
					It should now be much easier to configure.
				- The RichAI movement script now works a lot better with the RVOController.
					Previously the movement could be drastically different when the RVOController was used
					and local avoidance didn't work well when the agent was at the edge of the navmesh.
				- Improved gizmos for the RVOController.
				- Added RVOController.ForceSetVelocity to use when you want agents to avoid a player (or otherwise externally controlled) character.
				- RVO agents can now have different priorities, lower priority agents will avoid higher priority agents more.
				- The neighbour distance field is now automatically calculated. This makes it easier to configure the agents and it will
					also improve performance slightly when the agents are moving slowly (for example in very crowded scenarios).
				- Added support for grid graphs to \link Pathfinding.RVO.RVONavmesh RVONavmesh\endlink.
				- Added a new example scene for RVO in 2D
					\htmlonly <video class="tinyshadow" controls="true" loop="true"><source src="images/3vs4/rvo2d.mp4" type="video/mp4" /></video> \endhtmlonly
		- General
			- Huge increase in the performance of graph gizmos.
				This was accomplished by bypassing the Unity Gizmos and creating a custom gizmo rendererer that is able to retain
				the gizmo meshes instead of recreating them every frame (as well as using a lot fewer draw calls than Unity Gizmos).
				Therefore the graphs usually only need to check if the nodes have changed, and only if they have changed they will
				rebuild the gizmo meshes. <b>This may cause graph updates to seem like they introduce more lag than they actually do</b>
				since a graph update will also trigger a gizmo rebuild. So make sure to always profile with gizmos disabled.
				For a 1000*1000 graph, which previously almost froze the editor, the time per frame went from over 4200 ms to
				around 90 ms when no nodes had changed.
				\htmlonly <video class="tinyshadow" controls="true" loop="true"><source src="images/3vs4/gizmo_performance.mp4" type="video/mp4" /></video> \endhtmlonly
			- Improved the style of graph gizmos. A solid surface is now rendered instead of only the connections between the nodes.
				The previous mode of rendering only connections is of course still available.
				\shadowimage{3vs4/gizmos.png}
			- Added a new example scene showing how to configure hexagon graphs.
			- Added gizmos for hexagon graphs (grid graphs with certain settings).
				\shadowimage{3vs4/hexagon_thin.png}
			- Implemented async scanning. \link AstarPath.ScanAsync AstarPath.active.ScanAsync \endlink is an IEnumerable that can be iterated over several frames
				so that e.g a progress bar can be shown while calculating the graphs. Note that this does not guarantee
				a good framerate, but at least you can show a progress bar.
			- Improved behaviour of the AIPath movement script.
				- AIPath now works in the XY plane as well. In fact it works with any graph rotation.
					The Z axis is always the forward axis for the agent, so for 2D games with sprites you may have to attach the sprite
					to a child object which is rotated for it to show up correctly.
				- Previously the slowdownDistance had to be smaller than the forwardLook field otherwise the character
					could slow down even when it had not reached the end of the path.
				- The agent should stop much more precisely at the end of the path now.
				- The agent now rotates with a fixed angular speed instead of a varying one as this is often more realistic.
				- Reduced the likelihood of the agent spinning around when it reaches the end of the path.
				- It no longer uses the forwardLook variable.
					It was very tricky to set correctly, now the pickNextWaypointDist variable is used for everything instead
					and generally this should give you smoother movement.
			- Improved behaviour of the \link Pathfinding.RichAI RichAI \endlink movement script.
				- The agent should stop much more precisely at the end of the path now.
				- Reduced the likelihood of the agent spinning around when it reaches the end of the path.
			- Scanning the graph using AstarPath.Scan will now profile the various parts of the graph scanning
				process using the Unity profiler (Profiler.BeginSample and Profiler.EndSample).
			- \link Pathfinding.DynamicObstacle DynamicObstacle \endlink will now update the graph immediately if an object with that component is created during runtime
				instead of waiting until it was moved for the first time.
			- \link Pathfinding.GraphUpdateScene GraphUpdateScene \endlink and \link Pathfinding.GraphUpdateShape GraphUpdateShape \endlink can now handle rotated graphs a lot better.
				The rotation of the object the GraphUpdateScene component is attached to determines the 'up' direction for the shape
				and thus which points will be considered to be inside the shape.
				The world space option had to be removed from GraphUpdateScene because it didn't really work with rotated graphs.
				The lockToY option for GraphUpdateScene has also been removed because it wasn't very useful and after this change it would only have had an impact
				in rare cases.
			- Improved \link Pathfinding.GraphUpdateScene GraphUpdateScene \endlink editor. When editing the points in the scene view it now shows helper lines
				to indicate where a new point is going to be added and which other points it will connect to
				as well as several other minor improvements.
				\htmlonly <video class="tinyshadow" controls="true" loop="true"><source src="images/3vs4/graph_update_scene_sd.mp4" type="video/mp4" /></video> \endhtmlonly
			- \link Pathfinding.GraphUpdateScene GraphUpdateScene \endlink now supports using the bounds from 2D colliders and the shape from PolygonCollider2D.
			- Added opaqueness slider for the gizmos under Inspector -> Settings -> Colors.
			- Added \link Pathfinding.Path.BlockUntilCalculated Path.BlockUntilCalculated \endlink which is identical to AstarPath.BlockUntilCalculated.
			- Added Seeker.CancelCurrentPathRequest.
			- Added \link Pathfinding.NavGraph.GetNodes NavGraph.GetNodes(System.Action<GraphNode>) \endlink which calls a delegate with each node in the graph.
				Previously NavGraph.GetNodes(GraphNodeDelegateCancelable) existed which did the same thing but required the delegate
				to return true if it wanted the graph to continue calling it with more nodes. It turns out this functionality was very rarely needed.
			- Individual graphs can now be scanned using #AstarPath.Scan(NavGraph) and other related overloads.
			- Improved \link Pathfinding.BinaryHeap priority queue \endlink performance. On average results in about a 2% overall pathfinding performance increase.
			- ObjectPool<T> now requires a ref parameter when calling Release with an object to help prevent silly bugs.
			- 'Min Area Size' has been removed. The edge cases are now handled automatically.
			- Added ObjectPoolSimple<T> as a generic object pool (ObjectPool<T> also exists, but for that T must implement IAstarPooledObject).
			- \link Pathfinding.RaycastModifier RaycastModifier \endlink now supports multi editing.
			- Added \link Pathfinding.GraphNode.RandomPointOnSurface GraphNode.RandomPointOnSurface \endlink.
			- Added \link Pathfinding.GraphNode.SurfaceArea GraphNode.SurfaceArea \endlink.
			- Int2 and \link Pathfinding.Int3 Int3 \endlink now implement IEquatable for slightly better performance and fewer allocations in some places.
			- \link Pathfinding.Examples.LocalSpaceRichAI LocalSpaceRichAI \endlink can now be used with any rotation (even things like moving on an object that is upside down).
			- The \link Pathfinding.FunnelModifier funnel modifier \endlink can now handle arbitrary graphs (even graphs in the 2D plane) if the new unwrap option is enabled.
			- The \link Pathfinding.FunnelModifier funnel modifier \endlink can split the resulting path at each portal if the new \link Pathfinding.FunnelModifier.splitAtEveryPortal splitAtEveryPortal \endlink option is enabled.
		- Recast/Navmesh Graphs
			- Recast graph scanning is now multithreaded which can improve scan times significantly.
			- Recast graph scanning now handles large worlds with lots of objects better. This can improve scan times significantly.
			\htmlonly <video class="tinyshadow" controls="true" loop="true"><source src="images/3vs4/recast_scanning_performance.mp4" type="video/mp4" /></video> \endhtmlonly
			- Improved performance of nearest node queries for Recast/navmesh graphs.
			- Editing navmesh cut properties in the inspector now forces updates to happen immediately which makes editing easier.
			- Long edges in recast graphs are now split at tile borders as well as at obstacle borders.
				This can in particular help on terrain maps where the tile borders do not follow the elevation that well
				so the max edge length can be reduced to allow the border to follow the elevation of the terrain better.
			- Recast graphs can now be rotated arbitrarily.
				- Navmesh cutting still works!
				- The RichAI script currently does not support movement on rotated graphs, but the AIPath script does.
			- Improved performance of navmesh cutting for large worlds with many tiles and NavmeshAdd components.
			- Navmesh graphs and recast graphs now share the same base code which means that navmesh graphs
				now support everything that previously only recast graphs could be used for, for example
				navmesh cutting.
			- The NavmeshCut inspector now shows a warning if no TileHandlerHelper component is present in the scene.
				A TileHandlerHelper component is necessary for the NavmeshCuts to update the graphs.
			- Recast graphs now use less memory due to the BBTree class now using around 70% less memory per node.
			- Recast graphs now allocate slightly less memory when recalculating tiles or scanning the graph.
			- Cell height on Recast graphs is now automatically set to a good value.
			- Navmesh cutting is now a bit better at using object pooling to avoid allocations.
			- TileHandlerHelper now updates the tiles properly when one or multiple tiles on the recast graph are recalculated
				due to a graph update or because it was rescanned.
			- Navmesh cutting now uses more pooling to reduce allocations slightly.
			- Improved performance of loading and updating (using navmesh cutting) recast tiles with a large number of nodes.
		- Grid Graphs
			- Added LevelGridNode.XCoordinateInGrid, LevelGridNode.ZCoordinateInGrid, LevelGridNode.LayerCoordinateInGrid.
			- Added GridGraph.GetNodesInRegion(IntRect).
				Also works for layered grid graphs.
			- Layered grid graphs now have support for 'Erosion Uses Tags'.
			- Added GridGraph.CalculateConnections(GridNodeBase) which can be used for both grid graphs and layered grid graphs.
			- Grid graphs can now draw the surface and outline of the graph instead of just the connections between the nodes.
				The inspector now contains several toggles that can be used to switch between the different rendering modes.
			- The ProceduralGraphMover component now works with LayerGridGraph as well.
			- Added GridGraph.RecalculateCell(x,y) which works both for grid graphs and layered grid graphs.
				This replaces the UpdateNodePositionCollision method and that method is now deprecated.
			- Improved GridGraph.RelocateNodes which is now a lot more resilient against floating point errors.
			- Added dimetric (60°) to the list of default values for the isometric angle field on grid graphs.
			- Changing the width/depth of a grid graph will now keep the current pivot point at the same position instead of always keeping the bottom left corner fixed.
				(the pivot point can be changed to the center/bottom left/top left/top right/bottom right right next to the position field in the grid graph inspector)
			- Improved fluidity and stability when resizing a grid graph in the scene view.
				It now snaps to full node increments in size.
			- Grid graphs now display a faint grid pattern in the scene view even when the graph is not scanned
				to make it easier to position and resize the graph.
			- Improved styling of some help boxes in the grid graph inspector when using the dark UI skin.
			- The size of the unwalkable node gizmo (red cube) on grid graphs is now based on the node size to avoid the gizmos being much larger or much smaller than the nodes.
			- Implemented \link Pathfinding.ABPath.EndPointGridGraphSpecialCase special case for paths on grid graphs \endlink so that if you request a path to an unwalkable node with several
				walkable nodes around it, it will now not pick the closest walkable node to the requested target point and find a path to that
				but it will find the shortest path which goes to any of the walkable nodes around the unwalkable node.
				\htmlonly <a href="images/abpath_grid_not_special.gif">Before</a>, <a href="images/abpath_grid_special.gif">After</a> \endhtmlonly.
				This is a special case of the MultiTargetPath, for more complicated configurations of targets the multi target path needs to be used to be able to handle it correctly.
- Changes
		- Node connections are now represented using an array of structs (of type Connection) instead of
			one array for target nodes and one array for costs.
		- When scanning a graph in the editor, the progress bar is not displayed until at least 200 ms has passed.
			Since displaying the progress bar is pretty slow, this makes scanning small graphs feel more snappy.
		- GridGraph and LayerGridGraph classes now have a 'transform' field instead of a matrix and inverseMatrix fields.
			The GraphTransform class also has various other nice utilities.
		- Moved mesh collecting code for Recast graphs to a separate class to improve readability.
		- Refactored out large parts of the AstarPath class to separate smaller classes to improve readability and increase encapsulation.
		- AstarPath.RegisterSafeUpdate is now implemented using WorkItems. This yields a slightly different behavior (previously callbacks added using RegisterSafeUpdate would
			always be executed before work items), but that should rarely be something that you would depend on.
		- Replaced AstarPath.BlockUntilPathQueueBlocked with the more robust AstarPath.PausePathfinding method.
		- The default radius, height and center for RVOControllers is now 0.5, 2 and 1 respectively.
		- To reduce confusion. The second area color is now a greenish color instead of a red one.
			The red color would often be mistaken as indicating unwalkable nodes instead of simply a different connected component.
			Hopefully green will be a more neutral color.
		- Renamed AstarPath.astarData to AstarPath.data.
		- Renamed NavmeshCut.useRotation and NavmeshAdd.useRotation to useRotationAndScale (since they have always affected scale too).
		- Renamed GridGraph.GenerateMatrix to GridGraph.UpdateTransform to be consistent with recast/navmesh graphs.
			The GenerateMatrix method is now deprecated.
		- Renamed AstarPath.WaitForPath to AstarPath.BlockUntilCalculated.
		- Renamed GridNode.GetConnectionInternal to HasConnectionInDirection.
		- Renamed NNInfo.clampedPosition to NNInfo.position.
		- Renamed GridGraph.GetNodesInArea to GetNodesInRegion to avoid confusing the word 'area' for what is used to indicate different connected components in graphs.
		- Renamed AIPath.turningSpeed to \link Pathfinding.AIPath.rotationSpeed rotationSpeed\endlink.
		- Deprecated Seeker.GetNewPath.
		- Deprecated NavGraph.matrix, NavGraph.inverseMatrix, NavGraph.SetMatrix and NavGraph.RelocateNodes(Matrix4x4,Matrix4x4).
			They have been replaced with a single transform field only available on some graph types as well as a few other overloads of teh RelocateNodes method.
		- Changed the signature of NavGraph.GetNodes(GraphNodeDelegateCancelable) to the equivalent NavGraph.GetNodes(System.Func<GraphNode,bool>).
		- Replaced all instances of GraphNodeDelegate with the equivalent type System.Action<GraphNode>.
		- Made a large number of previously public methods internal to reduce confusion about which methods one should use in a class and make the documentation easier to read.
			In particular the Path class has had its set of public methods reduced a lot.
		- Made AstarData.AddGraph(NavGraph) private. Scripts should use AstarData.AddGraph(System.Type) instead.
		- Moved internal fields of NNInfo into a new NNInfoInternal struct to make the API easier to use. Previously NNInfo contained some internal fields, but now they are only in NNInfoInternal.
		- Moved GetNeighbourAlongDirection to GridNodeBase and made it public.
		- An overload of the GridGraph.CalculateConnections method has been made non-static.
		- LayerGridGraph.LinkedLevelNode and LayerGridGraph.LinkedLevelCell are now private classes since they are only used by the LayerGridGraph.
		- MonoModifier.OnDestroy is now a virtual function.
		- AstarPath.IsUsingMultithreading and NumbParallelThreads have been made non-static.
		- AstarPath.inGameDebugPath is now private.
		- AstarPath.lastScanTime is now read only.
		- Removed the 'climb axis' field from grid graphs. The axis is now automatically set to the graph's UP direction (which is
			the only direction that makes sense and all other directions can be transformed to this one anyway).
		- Removed the 'worldSpace' parameter from RecastGraph.ReplaceTile, it is no longer possible to supply world space vertices to
			that method since graph space vertices are required for some things.
		- Removed BBTree.QueryCircle and BBTree.Query since they were not used anywhere.
		- Removed the Path.searchIterations field because it wasn't very useful even as debug information.
		- Removed the Path.maxFrameTime field because it was not used.
		- Removed the Path.callTime property because it was not used.
		- Removed the ABPath.startHint, ABPath.endHint fields because they were not used.
		- Removed the ABPath.recalcStartEndCosts field because it was not used.
		- Removed the RecursiveBinary and RecursiveTrinary modes for RichAI.funnelSimplification because the Iterative mode
			was usually the best and fastest anyway (also the other two modes had a rare bug where they could get cought in infinite loops).
		- Removed the Polygon.Subdivide method because it was not used anywhere.
		- Removed the NavGraph.Awake method because it was not used for anything.
		- Removed ASTAR_OPTIMIZE_POOLING from Optimization tab. It is now always enabled in standalone builds and always disabled in the Unity editor.
		- Removed various unused Recast code.
		- Removed support for forcing the inspector skin to be dark or light. The value provided by EditorGUIUtility.isProSkin is always used now.
		- Removed multiplication operator for Int3 with a Vector3 because it is a nonstandard operation on vectors (and it is not that useful).
		- Removed the since long deprecated example script AIFollow.
		- Removed the AdaptiveSampling algorithm for local avoidance. Only GradientDescent is used now.
		- Removed empty PostProcess method in NavMeshGraph.
- Fixes
		- Fixed RichAI and AIPath trying to use CharacterControllers even if the CharacterController component was disabled.
		- Fixed rotated recast/navmesh graphs would ensure each node's vertices were laid out clockwise in XZ space instead of in graph space which could cause parts of the graph to become disconnected from the rest.
		- Fixed a bug where graphs could fail to be deserialized correctly if the graph list contained a null element
		- Fixed a bug where the json serializer could emit True/False instead of true/false which is the proper json formatting.
		- Fixed LayerGridGraphs' "max climb" setting not working properly with rotated graphs.
		- Fixed LayerGridGraphs' "character height" setting not working properly with rotated graphs.
		- Fixed LayerGridGraphs assuming there were no obstacles nearby if no ground was found.
		- Fixed DynamicObstacle getting caught in an infinite loop if there was no AstarPath component in the scene when it was created. Thanks MeiChen for finding the bug.
		- Fixed NodeLink2 deserialization causing exceptions if the node hadn't linked to anything when it was serialized. Thanks Skalev for finding the bug.
		- Fixed the AlternativePath modifier could crash the pathfinding threads if it logged a warning since it used the Debug.Log(message,object) overload which
			can only be used from the Unity thread.
		- Fixed an issue where layer mask fields in graph editors would show 'Nothing' if they only included layers which had no name set.
		- Fixed potential memory leak.
			Paths in the path pool would still store the callback which is called when the path has been calculated
			which that means it would implicitly hold a reference to the object which had the method that would be called.
			Thanks sehee for pointing this out.
		- Fixed GridNode.ClosestPointOnNode could sometimes return the wrong y coordinate relative to the graph (in particular when the graph was rotated) and the y coordinate would not snap to the node's surface.
		- Fixed AstarData.AddGraph would fill *all* empty slots in the graph array with the graph instead of just the first. Thanks bitwise for finding the bug.
		- Improved compatibility with Unity 5.5 which was need due to the newly introduced UnityEngine.Profiling namespace.
		- Fixed graph updates on LayeredGridGraphs not respecting GraphUpdateObject.resetPenaltyOnPhysics.
		- Fixed potential memory leak when calling RecalculateCell on a layered grid graph.
		- LevelGridNode.ContainsConnection now reports correct values (previously it would only check
			non-grid connections).
		- Fixed not being able to deserialize settings saved with some old versions of the A* Pathfinding Project.
		- Tweaked ListPool to avoid returning lists with a very large capacity when a small one was requested
			as this could cause performance problems since Clear is O(n) where n is the capacity (not the size of the list).
		- Fixed GraphUpdateScene causing 'The Grid Graph is not scanned, cannot update area' to be logged when exiting play mode.
		- Fixed scanning a recast graph could in very rare circumstances throw a 'You are trying to pool a list twice' exception due to a multithreading
			race condition.
		- Fixed recast/navmesh graphs could return the wrong node as the closest one in rare cases, especially near tile borders.
		- Fixed another case of recast/navmesh graphs in rare cases returning the wrong node as the closest one.
		- Fixed gizmo drawing with 'Show Search Tree' enabled sometimes right after graph updates drawing nodes outside the
			search tree as if they were included in it due to leftover data from graph updates.
		- Fixed navmesh and recast graphs would unnecessarily be serialized by Unity which would slow down the inspector slightly.
		- Fixed AstarEnumFlagDrawer not working with private fields that used the [SerializeField] attribute.
			This does not impact anything that the A* Pathfinding Project used, but some users are using the AstarEnumFlagDrawer for
			other fields in their projects. Thanks Skalev for the patch.
		- Clicking 'Apply' in the Optimizations tab will now always refresh the UI instead of assuming that
			a recompilation will happen (it will not happen if only defines for other platforms than the current one were modified).
		- Fixed not being able to multi-edit RVOSquareObstacle components.
		- Fixed GridNode.ClearConnections(true) not removing all reversed connections and could sometimes remove the wrong ones.
		- Fixed TileHandlerHelper regularly checking for if an update needs to be done even if TileHandlerHelper.updateInterval was negative
			even though the documentation specifies that it should not do that (it only disabled updates when updateInterval = -1).
		- Fixed PathUtilities.GetPointsAroundPointWorld and PathUtilities.GetPointsAroundPoint returning incorrect results sometimes.
		- Fixed Path.immediateCallback not being reset to null when using path pooling.
		- TileHandlerHelper will now work even if Scan On Awake in A* Inspector -> Settings is false and you are scanning the graph later.
		- Fixed AstarWorkItem.init could be called multiple times.
		- Fixed some documentation typos.
		- Fixed colliders being included twice in the recast rasterization if the GameObject had a RecastNavmeshModifier attached to it which effectively made RecastNavmeshModifier not work well at all with colliders.
		- Fixed inspector for RecastNavmeshModifier not updating if changes were done to the fields by a script or when an undo or redo was done.
		- Fixed SimpleSmoothModifier custom editor would sometimes set all instances of a field to the same value
			when editing multiple objects at the same time.
		- Fixed division by zero when the TimeScale was zero in the AstarDebugger class. Thanks Booil Jung for reporting the issue.
		- Various other small fixes in the AstarDebugger class.
		- Fixed division by zero when generating a recast graph and the cell size was much larger than the bounds of the graph.
		- Fixed the recast graph data structures could be invalid while a graph update was running in a separate thread.
			This could cause API calls like AstarPath.GetNearest to throw exceptions. Now the affected tiles are recalculated
			in a separate thread and then the updates are applied to the existing graph in the Unity thread.
		- Fixed some cases where the AlternativePath modifier would apply penalties incorrectly and possibly crash the pathfinding thread.
		- Fixed IAgent.NeighbourCount would sometimes not be reset to 0 when the agent was locked and thus takes into account no other agents.
		- Fixed RVO threads would sometimes not be terminated which could lead to memory leaks if switching scenes a lot.
		- Fixed GridGraph.GetNearest and NavGraph.GetNearest not handling constraint=null.
- Internal changes
		- These are changes to the internals of the system and will most likely not have any significant externally visible effects.
		- Removed some wrapper methods for the heap in the PathHandler class since they were just unnecessary. Exposed the heap field as readonly instead.
		- Renamed BinaryHeapM to BinaryHeap.
		- Renamed ExtraMesh to RasterizationMesh.
		- Refactored TileHandler.CutPoly to reduce code messiness and also fixed some edge case bugs.
- Documentation
		- Among other things: improved the \ref writing-graph-generators guide (among other things it no longer uses hard to understand calculations to get the index of each node).

## 3.8.8.1 (2017-01-12)
- Fixes
		- Fixed the 'Optimization' tab sometimes logging errors when clicking Apply on Unity 5.4 and higher.
		- More UWP fixes (pro version only).

## 3.8.8 (2017-01-11)
- Fixes
		- Fixed errors when deploying for the Universal Windows Platform (UWP).
			This includes the Hololens platform.
		- It is no longer necessary to use the compiler directive ASTAR_NO_ZIP when deploying for UWP.
			zipping will be handled by the System.IO.Compression.ZipArchive class on those platforms (ZipArchive is not available on other platforms).
			If you have previously enabled ASTAR_NO_ZIP it will stay enabled to ensure compatibility.
		- Changed some comments from the '

## 3.8.7 (2016-11-26)
- Fixes
		- Improved compatibility with Unity 5.5 which was needed due to the newly introduced UnityEngine.Profiling namespace.

## 3.8.6 (2016-10-31)
- Upgrade Notes
		- Note that a few features and some fixes that have been available in the beta releases are not
			included in this version because they were either not ready to be released or depended on other
			changes that were not ready.
		- Dropped support for Unity 5.1.
		- Moved some things to inside the Pathfinding namespace to avoid naming collisions with other packages.
			Make sure you have the line 'using Pathfinding;' at the top of your scripts.
		- Seeker.StartMultiTargetPath will now also set the enabledTags and tagPenalties fields on the path.
			Similar to what StartPath has done. This has been the intended behaviour from the start, but bugs happen.
			See http://forum.arongranberg.com/t/multitargetpath-doesnt-support-tag-constraints/2561/3
		- The JsonFx library is no longer used, so the Pathfinding.JsonFx.dll file in the plugins folder
			may be removed to reduce the build size a bit. UnityPackages cannot delete files, so you have to delete it manually.
		- RecastGraph.UpdateArea (along with a few other functions) is now explicitly implemented for the IUpdatableGraph interface
			as it is usually a bad idea to try to call those methods directly (use AstarPath.UpdateGraphs instead).
		- AstarPath.FlushWorkItems previously had pretty bad default values for the optional parameters.
			By default it would not necessarily complete all work items, it would just complete those that
			took a single frame. This is pretty much never what you actually want so to avoid
			confusion the default value has been changed.
- New Features and Improvements
		- The JsonFx library is no longer used. Instead a very tiny json serializer and deserializer has been written.
			In addition to reducing code size and being slightly faster, it also means that users using Windows Phone
			no longer have to use the ASTAR_NO_JSON compiler directive. I do not have access to a windows phone
			however, so I have not tested to build it for that platform. If any issues arise I would appreciate if
			you post them in the forum.
		- Improved inspector for NavmeshCut.
		- NodeLink2 can now be used even when using cached startup or when loading serialized data in other ways just as long as the NodeLink2 components are still in the scene.
		- LevelGridNode now has support for custom non-grid connections (just like GridNode has).
		- Added GridNode.XCoordinateInGrid and GridNode.ZCoordinateInGrid.
		- Improved documentation for GraphUpdateShape a bit.
- Changes
		- Removed EditorUtilities.GetMd5Hash since it was not used anywhere.
		- Deprecated TileHandler.GetTileType and TileHandler.GetTileTypeCount.
		- Seeker.StartPath now properly handles MultiTargetPath objects as well.
		- Seeker.StartMultiTargetPath is now deprecated. Note that it will now also set the
			enabledTags and tagPenalties fields on the path. Similar to what StartPath has done.
		- Removed GridGraph.bounds since it was not used or set anywhere.
		- GraphNode.AddConnection will now throw an ArgumentNullException if you try to call it with a null target node.
		- Made PointGraph.AddChildren and PointGraph.CountChildren protected since it makes no sense for them to be called by other scripts.
		- Changed how the 'Save & Load' tab looks to make it easier to use.
		- Renamed 'Path Debug Mode' to 'Graph Coloring' and 'Path Log Mode' to 'Path Logging' in the inspector.
		- RecastGraph.UpdateArea (along with a few other functions) is now explicitly implemented for the IUpdatableGraph interface
			as it is usually a bad idea to try to call those methods directly (use AstarPath.UpdateGraphs instead).
		- Removed ConnectionType enum since it was not used anywhere.
		- Removed NodeDelegate and GetNextTargetDelegate since they were not used anywhere.
- Fixes
		- Fixed TinyJson not using culture invariant float parsing and printing.
			This could cause deserialization errors on systems that formatted floats differently.
		- Fixed the EndingCondition example script.
		- Fixed speed being multiplied by Time.deltaTime in the AI script in the get started tutorial when it shouldn't have been.
		- Fixed FunnelModifier could for some very short paths return a straight line even though a corner should have been inserted.
		- Fixed typo. 'Descent' (as in 'Gradient Descent') was spelled as 'Decent' in some cases. Thanks Brad Grimm for finding the typo.
		- Fixed some documentation typos.
		- Fixed some edge cases in RandomPath and FleePath where a node outside the valid range of G scores could be picked in some cases (when it was not necessary to do so).
		- Fixed editor scripts in some cases changing the editor gui styles instead of copying them which could result in headers in unrelated places in the Unity UI had the wrong sizes. Thanks HP for reporting the bug.
		- Fixed NavmeshCut causing errors when cutting the navmesh if it was rotated upside down or scaled with a negative scale.
		- Fixed TriangleMeshNode.ClosestPointOnNodeXZ could sometimes return the wrong point (still on the node surface however).
			This could lead to characters (esp. when using the RichAI component) teleporting in rare cases. Thanks LordCecil for reporting the bug.
		- Fixed GridNodes not serializing custom connections.
		- Fixed nodes could potentially get incorrect graph indices assigned when additive loading was used.
		- Added proper error message when trying to call RecastGraph.ReplaceTile with a vertex count higher than the upper limit.
- Known Bugs
		- Calling GetNearest when a recast graph is currently being updated on another thread may in some cases result in a null reference exception
			being thrown. This does not impact navmesh cutting. This bug has been present (but not discovered) in previous releases as well.
		- Calling GetNearest on point graphs with 'optimizeForSparseGraph' enabled may in some edge cases return the wrong node as being the closest one.
			It will not be widely off target though and the issue is pretty rare, so for real world use cases it should be fine.
			This bug has been present (but not discovered) in previous releases as well.

## 3.8.3 through 3.8.5 were beta versions

## 3.8.2 (2016-02-29)
- Improvements
		- DynamicObstacle now handles rotation and scaling better.
		- Reduced allocations due to coroutines in DynamicObstacle.
- Fixes
		- Fixed AstarPath.limitGraphUpdates not working properly most of the time.
			In order to keep the most common behaviour after the upgrade, the value of this field will be reset to false when upgrading.
		- Fixed DynamicObstacle not setting the correct bounds at start, so the first move of an object with the DynamicObstacle
			component could leave some nodes unwalkable even though they should not be. Thanks Dima for reporting the bug.
		- Fixed DynamicObstacle stopping to work after the GameObject it is attached to is deactivated and then activated again.
		- Fixed RVOController not working after reloading the scene due to the C# '??' operator not being equivalent to checking
			for '== null' (it doesn't use Unity's special comparison check). Thanks Khan-amil for reporting the bug.
		- Fixed typo in documentation for ProceduralGraphMover.floodFill.
- Changes
		- Renamed 'Max Update Frequency' to 'Max Update Interval' in the editor since it has the unit [second], not [1/second].
		- Renamed AstarPath.limitGraphUpdates to AstarPath.batchGraphUpdates and AstarPath.maxGraphUpdateFreq to AstarPath.graphUpdateBatchingInterval.
			Hopefully these new names are more descriptive. The documentation for the fields has also been improved slightly.

## 3.8.1 (2016-02-17)
- Improvements
		- The tag visualization mode for graphs can now use the custom list of colors
			that can be configured in the inspector.
			Thanks Arakade for the patch.
- Fixes
		- Recast graphs now handle meshes and colliders with negative scales correctly.
			Thanks bvance and peted for reporting it.
		- Fixed GridGraphEditor throwing exceptions when a user had created a custom grid graph class
			which inherits from GridGraph.
		- Fixed Seeker.postProcessPath not being called properly.
			Instead it would throw an exception if the postProcessPath delegate was set to a non-null value.
			Thanks CodeSpeaker for finding the bug.

## 3.8 (2016-02-16)
- The last version released on the Unity Asset Store was 3.7, so if you are upgrading
		from that version check out the release notes for 3.7.1 through 3.7.5 as well.
- Breaking Changes
		- For the few users that have written their own Path Modifiers. The 'source' parameter to the Apply method has been removed from the IPathModifier interface.
			You will need to remove that parameter from your modifiers as well.
		- Modifier priorities have been removed and the priorities are now set to sensible hard coded values since at least for the
			included modifiers there really is only one ordering that makes sense (hopefully there is no use case I have forgotten).
			This may affect your paths if you have used some other modifier order.
			Hopefully this change will reduce confusion for new users.
- New Features and Improvements
		- Added NodeConnection mode to the StartEndModifier on the Seeker component.
			This mode will snap the start/end point to a point on the connections of the start/end node.
			Similar to the Interpolate mode, but more often does what you actually want.
		- SimpleSmoothModifier now has support for multi editing.
		- Added a new movement script called AILerp which uses linear interpolation to follow the path.
			This is good for games which want the agent to follow the path exactly and not use any
			physics like behaviour. This movement script works in both 2D and 3D.
		- Added a new 2D example scene which uses the new AILerp movement script.
		- All scripts now have a <a href="http://docs.unity3d.com/ScriptReference/HelpURLAttribute.html">HelpURLAttribute</a>
			so the documentation button at the top left corner of every script inspector now links directly to the documentation.
		- Recast graphs can now draw the surface of a navmesh in the scene view instead of only
			the node outlines. Enable it by checking the 'Show mesh surface' toggle in the inspector.
			Drawing the surface instead of the node outlines is usually faster since it does not use
			Unity Gizmos which have to rebuild the mesh every frame.
		- Improved GUI for the tag mask field on the Seeker.
		- All code is now consistently formatted, utilising the excellent Uncrustify tool.
		- Added animated gifs to the \link Pathfinding.RecastGraph.cellSize Recast graph \endlink documentation showing how some parameters change the resulting navmesh.
			If users like this, I will probably follow up and add similar gifs for variables in other classes.
			\shadowimage{recast/character_radius.gif}
- Fixes
		- Fixed objects in recast graphs being rasterized with an 0.5 voxel offset.
			Note that this will change how your navmesh is rasterized (but usually for the better), so you may want to make sure it still looks good.
		- Fixed graph updates to navmesh and recast graphs not checking against the y coordinate of the bounding box properly (introduced in 3.7.5).
		- Fixed potential bug when loading graphs from a file and one or more of the graphs were null.
		- Fixed invalid data being saved when calling AstarSerializer.SerializeGraphs with an array that was not equal to the AstarData.graphs array.
			The AstarSerializer is mostly used internally (and internally it is always called with the AstarData.graphs array). Thanks munkman for reporting this.
		- Fixed incorrect documentation for GridNode.NodeInGridIndex. Thanks mfjk for reporting it!
		- Fixed typo in a recast graph log message (where -> were). Thanks bigdaddio for reporting it!
		- Fixed not making sure the file is writable before writing graph cache files (Perforce could sometimes make it read-only). Thanks Jørgen Tjernø for the patch.
		- Fixed RVOController always using FindObjectOfType during Awake, causing performance issues in large scenes. Thanks Jørgen Tjernø for the patch.
		- Removed QuadtreeGraph, AstarParallel, NavMeshRenderer and NavmeshController from the released version.
			These were internal dev files but due to typos they had been included in the release.
			It will also automatically refresh itself if the graph has been rescanned with a different number of tiles.
		- Fixed SimpleSmoothModifier not always including the exact start point of the path.
		- Fixed ASTAR_GRID_NO_CUSTOM_CONNECTIONS being stripped out of the final build, so that entry in the Optimizations tab didn't actually do anything.
		- Fixed performance issue with path pooling. If many paths were being calculated and pooled, the performance could be
			severely reduced unless ASTAR_OPTIMIZE_POOLING was enabled (which it was not by default).
		- Fixed 3 compiler warnings about using some deprecated Unity methods.
- Changes
		- Recast graphs' 'Snap To Scene' button now snaps to the whole scene instead of the objects that intersect the bounds that are already set.
			This has been a widely requested change. Thanks Jørgen Tjernø for the patch.
		- Moved various AstarMath functions to the new class VectorMath and renamed some of them to reduce confusion.
		- Removed various AstarMath functions because they were either not used or they already exist in e.g Mathf or System.Math.
			DistancePointSegment2, ComputeVertexHash, Hermite, MapToRange, FormatBytes,
			MagnitudeXZ, Repeat, Abs, Min, Max, Sign, Clamp, Clamp01, Lerp, RoundToInt.
		- PathEndingCondition (used with XPath) is now abstract since it doesn't really make any sense to use the default implementation (always returns true).
		- A 'Recyle' method is no longer required on path classes (reduced boilerplate).
		- Removed old IFunnelGraph interface since it was not used by anything.
		- Removed old ConvexMeshNode class since it was not used by anything.
		- Removed old script NavmeshController since it has been disabled since a few versions.
		- Removed Int3.DivBy2, Int3.unsafeSqrMagnitude and Int3.NormalizeTo since they were not used anywere.
		- Removed Int2.sqrMagnitude, Int2.Dot since they were not used anywhere and are prone to overflow (use sqrMagnitudeLong/DotLong instead)
		- Deprecated Int2.Rotate since it was not used anywhere.
		- Deprecated Int3.worldMagnitude since it was not used anywhere.

## 3.7.5 (2015-10-05)
- Breaking changes
		- Graph updates to navmesh and recast graphs now also check that the nodes are contained in the supplied bounding box on the Y axis.
			If the bounds you have been using were very short along the Y axis, you may have to change them so that they cover the nodes they should update.
- Improvements
		- Added GridNode.ClosestPointOnNode.
		- Optimized GridGraph.CalculateConnections by approximately 20%.
			This means slightly faster scans and graph updates.
- Changes
		- Graph updates to navmesh and recast graphs now also check that the nodes are contained in the supplied bounding box on the Y axis.
			If the bounds you have been using were very short along the Y axis, you may have to change them so that they cover the nodes they should update.
- Fixes
		- Fixed stack overflow exception when a pivot root with no children was assigned in the heuristic optimization settings.
		- Fixed scanning in the editor could sometimes throw exceptions on new versions of Unity.
			Exceptions contained the message "Trying to initialize a node when it is not safe to initialize any node".
			This happened because Unity changed the EditorGUIUtility.DisplayProgressBar function to also call
			OnSceneGUI and OnDrawGizmos and that interfered with the scanning.
		- Fixed paths could be returned with invalid nodes if the path was calculated right
			before a call to AstarPath.Scan() was done. This could result in
			the funnel modifier becoming really confused and returning a straight line to the
			target instead of avoiding obstacles.
		- Fixed sometimes not being able to use the Optimizations tab on newer versions of Unity.

## 3.7.4 (2015-09-13)
- Changes
		- AIPath now uses the cached transform field in all cases for slightly better performance.
- Fixes
		- Fixed recast/navmesh graphs could in rare cases think that a point on the navmesh was
		   in fact not on the navmesh which could cause odd paths and agents teleporting short distances.
- Documentation Fixes
		- Fixed the Seeker class not appearing in the documentation due to a bug in Doxygen (documentation generator).

## 3.7.3 (2015-08-18)
- Fixed GridGraph->Unwalkable When No Ground used the negated value (true meant false and false meant true).
		This bug was introduced in 3.7 when some code was refactored. Thanks DrowningMonkeys for reporting it.

## 3.7.2 (2015-08-06)
- Fixed penalties not working on navmesh based graphs (navmesh graphs and recast graphs) due to incorrectly configured compiler directives.
- Removed undocumented compiler directive ASTAR_CONSTANT_PENALTY and replaced with ASTAR_NO_TRAVERSAL_COST which
		can strip out code handling penalties to get slightly better pathfinding performance (still not documented though as it is not really a big performance boost).

## 3.7.1 (2015-08-01)
- Removed a few cases where exceptions where needed to better support WebGL when exception handling is disabled.
- Fixed MultiTargetPath could return the wrong path if the target of the path was the same as the start point.
- Fixed MultiTargetPath could sometimes throw exceptions when using more than one pathfinding thread.
- MultiTargetPath will now set path and vectorPath to the shortest path even if pathsForAll is true.
- The log output for MultiTargetPath now contains the length (in nodes) of the shortest path.
- Fixed RecastGraph throwing exceptions when trying to rasterize trees with missing (null) prefabs. Now they will simply be ignored.
- Removed RecastGraph.bbTree since it was not used for anything (bbTrees are stored inside each tile since a few versions)
- Improved performance of loading and updating large recast graph tiles (improved performance of internal AABB tree).
- Removed support for the compiler directive ASTAR_OLD_BBTREE.

## 3.7 (2015-07-22)
- The last version that was released on the Unity Asset Store
	  was version 3.6 so if you are upgrading from that version also check out the release
	  notes for 3.6.1 through 3.6.7.
- Upgrade notes
		- ProceduralGraphMover.updateDistance is now in nodes instead of world units since this value
		   is a lot less world scale dependant. So the defaults should fit more cases.
		   You may have to adjust it slightly.
		- Some old parts of the API that has been marked as deprecated long ago have been removed (see below).
		   Some other unused parts of the API that mostly lead to confusion have been removed as well.
- Improvements
		- Rewrote several documentation pages to try to explain concepts better and fixed some old code.
			- \ref accessing-data
			- \ref graph-updates
			- \ref writing-graph-generators
			- Pathfinding.NavmeshCut
			- And some other smaller changes.
		- Added an overload of Pathfinding.PathUtilities.IsPathPossible which takes a tag mask.
		- \link Pathfinding.XPath XPath \endlink now works again.
		- The ProceduralGraphMover component now supports rotated graphs (and all other ways you can transform it, e.g isometric angle and aspect ratio).
		- Rewrote GridGraph.Linecast to be more accurate and more performant.
			Previously it used a sampling approach which could cut corners of obstacles slightly and was pretty inefficient.
		- Linted lots of files to remove trailing whitespace, fix imports, use 'var' when relevant and various other small tweaks.
		- Added AstarData.layerGridGraph shortcut.
- Fixes
		- Fixed compilation errors for Windows Store.
			The errors mentioned ThreadPriority and VolatileRead.
		- Fixed LayerGridGraph.GetNearest sometimes returning the wrong node inside a cell (e.g sometimes it would always return the node with the highest y coordinate).\n
			This did not happen when the node size was close to 1 and the grid was positioned close to the origin.
			Which it of course was in all my tests (tests are improved now).
		- Fixed GridGraph.Linecast always returning false (no obstacles) when the start point and end point was the same.
			Now it returns true (obstacle) if the start point was inside an obstacle which makes more sense.
		- Linecasts on layered grid graphs now use the same implementation as the normal grid graph.\n
			This fixed a TON of bugs. If you relied on the old (buggy) behaviour you might have to change your algorithms a bit.
			It will now report more accurate hit information as well.
		- Fixed documentation on LayerGridGraph.Linecast saying that it would return false if there was an obstacle in the way
			when in fact exactly the opposite was true.
		- Fixed inspector GUI throwing exceptions when two or more grid graphs or layered grid graphs were visible and thickRaycast was enabled on only one of them.
		- Fixed a few options only relevant for grid graphs were visible in the layered grid graph inspector as well.
		- Fixed GridGraph.CheckConnection returned the wrong result when neighbours was Four and dir was less than 4.
		- All compiler directives in the Optimizations tab are now tested during the package build phase. So hopefully none of them should give compiler errors now.
		- Improved accuracy of intellisense.
		- Fixed the editor sometimes incorrectly comparing versions which could cause the 'New Update' window to appear even though no new version was available.
- Changes
		- Removed code only necessary for compatibility with Unity 4.5 and lower.
		- Removed a lot of internal unused old code.
		- Renamed GridGraph.GetNodePosition to GridGraph.GraphPointToWorld to avoid confusion.
		- Renamed 3rd party plugin license files to prevent the Unity Asset Store
			from detecting those as the license for the whole package.
		- Changed Seeker.traversableTags to be a simple int instead of a class.
		- GridNode and LevelGridNode now inherit from a shared base class called GridNodeBase.
		- Removed support for the compiler directive ConfigureTagsAsMultiple since it was not supported by the whole codebase
			and it was pretty old.
		- Marked a few methods in AstarData as deprecated since they used strings instead of types.
			If string to type conversion is needed it should be done elsewhere.
		- Removed some methods which have been marked as obsolete for a very long time.
			- AstarData.GetNode
			- PathModifier and MonoModifier.ApplyOriginal
			- Some old variants of PathModifier.Apply
			- GridGeneratorEditor.ResourcesField
			- Int3.safeMagnitude and safeSqrMagnitude
			- GraphUpdateUtilities.IsPathPossible (this has been since long been moved to the PathUtilities class)
			- All constructors on path classes. The static Construct method should be used instead since that can handle path pooling.
			- GraphNode.Position, walkable, tags, graphIndex. These had small changes made to their names (if they use upper- or lowercase letters) a long time ago.
				(for better or for worse, but I want to avoid changing the names now again to avoid breaking peoples' code)
			- GridNode.GetIndex.
		- Removed the Node class which has been marked as obsolete a very long time. This class has been renamed to GraphNode to avoid name conflicts.
		- Removed LocalAvoidanceMover which has been marked as obsolete a very long time. The RVO system has replaced it.
		- Removed Seeker.ModifierPass.PostProcessOriginal since it was not used. This also caused Seeker.postProcessOriginalPath to be removed.
		- Removed support for ASTAR_MORE_PATH_IDS because it wasn't really useful, it only increased the memory usage.
		- Removed Path.height, radius, turnRadius, walkabilityMask and speed since they were dummy variables that have not been used and are
			better implemented using inheritance anyway. This is also done to reduce confusion for users.
		- Removed the old local avoidance system which has long since been marked as obsolete and replaced by the RVO based system.

## 3.6.7 (2015-06-08)
- Fixes
		- Fixed a race condition when OnPathPreSearch and OnPathPostSearch were called.
			When the AlternativePath modifier was used, this could cause the pathfinding threads to crash with a null reference exception.

## 3.6.6 (2015-05-27)
- Improvements
		- Point Graphs are now supported when using ASTAR_NO_JSON.
		- The Optimizations tab now modifies the player settings instead of changing the source files.
			This is more stable and your settings are now preserved even when you upgrade the system.
		- The Optimizations tab now works regardless of the directory you have installed the package in.
			Hopefully the whole project is now directory agnostic, but you never know.
- Changes
		- Switched out OnVoidDelegate for System.Action.
			You might get a compiler error because of this (for the few that use it)
			but then just rename your delegate to System.Action.
- Fixes
		- Fixed recast graphs not saving all fields when using ASTAR_NO_JSON.

## 3.6.5 (2015-05-19)
- Fixes
		- Fixed recast graphs generating odd navmeshes on non-square terrains.
		- Fixed serialization sometimes failing with the error 'Argument cannot be null' when ASTAR_NO_JSON was enabled.
		- The 'Walkable Climb' setting on recast graphs is now clamped to be at most equal to 'Walkable Height' because
			otherwise the navmesh generation can fail in some rare cases.
- Changes
		- Recast graphs now show unwalkable nodes with a red outline instead of their normal colors.

## 3.6.4 (2015-04-19)
- Fixes
		- Improved compatibility with WIIU and other big-endian platforms.

## 3.6.3 (2015-04-19)
- Fixes
		- Fixed RVONavmesh not adding obstacles correctly (they were added added, but all agents ignored them).

## 3.6.2 (2015-04-14)
- Fixes
		- Fixed null reference exception in the PointGraph OnDrawGizmos method.
		- Fixed a few example scene errors in Unity 5.

## 3.6.1 (2015-04-06)
- Upgrade notes:
		- The behaviour of NavGraph.RelocateNodes has changed.
			The oldMatrix was previously treated as the newMatrix and vice versa so you might
			need to switch the order of your parameters if you are calling it.
- Highlights:
		- Works in WebGL/IL2CPP (Unity 5.0.0p3).
			At least according to my limited tests.
		- Implemented RelocateNodes for recast graphs (however it cannot be used on tiled recast graphs).
		- Added support for hexagon graphs.
			Enable it by changing the 'Connections' field on a grid graph to 'Six'.
		- Fixed AstarData.DeserializeGraphsAdditive (thanks tmcsweeney).
		- Fixed pathfinding threads sometimes not terminating correctly.
			This would show up as a 'Could not terminate pathfinding thread...' error message.
		- Added a version of GridGraph.RelocateNodes which takes grid settings instead of a matrix for ease of use.
- Changes:
		- Removed NavGraph.SafeOnDestroy
		- Removed GridGraph.scans because it is a pretty useless variable.
		- Removed NavGraph.CreateNodes (and overriden methods) since they were not used.
		- Made GridGraph.RemoveGridGraphFromStatic private.
		- Removed NavMeshGraph.DeserializeMeshNodes since it was not used.
		- Made Seeker.lastCompletedVectorPath, lastCompletedNodePath, OnPathComplete, OnMultiPathComplete, OnPartialPathComplete
			private since they really shouldn't be used by other scripts.
		- Removed Seeker.saveGetNearestHints, Seeker.startHint, Seeker.endHint, Seeker.DelayPathStart since they were not used.
		- Removed unused methods of little use: AstarData.GuidToIndex and AstarData.GuidToGraph.
		- Removed RecastGraph.vertices and RecastGraph.vectorVertices since they were obsolete and not used.
		- Removed some old Unity 4.3 and Unity 3 compatibility code.
		- Recast graphs' 'Snap to scene' button now takes into account the layer mask and the tag mask when snapping, it now also checks terrains and colliders instead of just meshes (thanks Kieran).
- Fixes:
		- Fixed RecastGraph bounds gizmos could sometimes be drawn with the wrong color.
		- Fixed a rare data race which would cause an exception with the message
			'Trying to initialize a node when it is not safe to initialize any nodes' to be thrown
		- Tweaked Undo behaviour, should be more stable now.
		- Fixed grid graph editor changing the center field very little every frame (floating point errors)
			causing an excessive amount of undo items to be created.
		- Reduced unecessary dirtying of the scene (thanks Ben Hymers).
		- Fixed RVOCoreSimulator.WallThickness (thanks tmcsweeney).
		- Fixed recast graph not properly checking for the case where an object had a MeshFilter but no Renderer (thanks 3rinJax).
		- Fixed disabling ASTAR_RECAST_ARRAY_BASED_LINKED_LIST (now ASTAR_RECAST_CLASS_BASED_LINKED_LIST) would cause compiler errors.
		- Fixed recast graphs could sometimes voxelize the world incorrectly and the resulting navmesh would have artifacts.
		- Fixed graphMask code having been removed from the free version in some cases
			due to old code which treated it as a pro only feature.
		- Improved compatibility with Xbox One.
		- Fixed RVOController layer field not working when multiple agents were selected.
		- Fixed grid nodes not being able to have custom connections in the free version.
		- Fixed runtime error on PS4.

## 3.6 (2015-02-02)
- Upgrade notes:
		- Cache data for faster startup is now stored in a separate file.\n
			This reduces the huge lag some users have been experiencing since Unity changed their Undo system.\n
			You will need to open the AstarPath components which used cached startup, go to the save and load tab
			and press a button labeled "Transfer cache data to a separate file".
- Highlights:
		- Added support for the Jump Point Search algorithm on grid graphs (pro only).\n
			The JPS algorithm can be used to speed up pathfinding on grid graphs *without any penalties or tag weights applied* (it only works on uniformly weighted graphs).
			It can be several times faster than normal A*.
			It works best on open areas.
		- Added support for heuristic optimizations (pro only).\n
			This can be applied on any static graph, i.e any graph which does not change.
			It requires a rather slow preprocessing step so graph updates will be really slow when using this.
			However when the preprocessing is done, it can speed up pathfinding with an order of magnitude.
			It works especially well in mazes with lots of options and dead ends.\n
			Combined with JPS (mentioned above) I have seen it perform up to 20x better than regular A* with no heuristic optimizations.
		- Added PointNode.gameObject which will contain the GameObject each node was created from.
		- Added support for RVO obstacles.\n
			It is by no means perfect at this point, but at least it works.
		- Undo works reasonably well again.\n
			It took a lot of time working around weird Unity behaviours.
			For example Unity seems to send undo events when dragging items to object fields (why? no idea).
		- Dragging meshes to the NavmeshGraph.SourceMesh field works again.\n
			See fix about undo above.
		- Extended the max number of possible areas (connected components) to 2^17 = 131072 up from 2^10 = 1024.\n
			No memory usage increase, just shuffling bits around.\n
			Deprecated compiler directive ASTAR_MORE_AREAS
		- Extended the max number of graphs in the inspector to 256 up from 4 or 32 depending on settings.\n
			No memory usage increase, just shuffling bits around.
			I still don't recommend that you actually use this many graphs.
		- Added RecastTileUpdate and RecastTileUpdateHandler scripts for easier recast tile updating with good performance.
		- When using A* Inspector -> Settings -> Debug -> Path Debug Mode = {G,F,H,Penalties}
			you previously had to set the limits for what should be displayed as "red" in the scene view yourself, this is now
			optionally automatically calculated. The UI for it has also been improved.
- Improvements:
		- Added penaltyAnglePower to Grid Graph -> Extra -> Penalty from Angle.\n
			This can be used to increase the penalty even more for large angles than for small angles (more than it already does, that is).
		- ASTAR_NO_JSON now works for recast graphs as well.
		- Added custom inspector for RecastNavmeshModifier, hopefully it will not be as confusing anymore.
- Changes:
		- FleePath now has a default flee strength of 1 to avoid confusion when the FleePath doesn't seem to flee from anything.
		- Removed some irrelevant defines from the Optimizations tab.
		- IAgent.Position cannot be changed anymore, instead use the Teleport and SetYPosition methods.
		- Exposed GraphUpdateObject.changedNodes.
		- Deprecated the threadSafe paremeter on RegisterSafeUpdate, it is always treated as true now.
		- The default value for AstarPath.minAreaSize is now 0 since the number of areas (connected component) indices has been greatly increased (see highlights).
		- Tweaked ProceduralWorld script (used for the "Procedural" example scene) to reduce FPS drops.
- Fixes:
		- AstarPath.FlushGraphUpdates will now complete all graph updates instead of just making sure they have started.\n
			In addition to avoiding confusion, this fixes a rare null reference exception which could happen when using
			the GraphUpdateUtilities.UpdateGraphsNoBlock method.
		- Fixed some cases where updating recast graphs could throw exceptions. (message begun with "No Voxelizer object. UpdateAreaInit...")
		- Fixed typo in RVOSimulator. desiredSimulatonFPS -> desiredSimulationFPS.
		- RVO agents move smoother now (previously their velocity could change widely depending on the fps, the average velocity was correct however)
		- Fixed an exception which could, with some graph settings, be thrown when deserializing on iPhone when bytecode stripping was enabled.
		- Fixed a NullReferenceException in MultiTargetPath which was thrown if the path debug mode was set to "Heavy".
		- Fixed PathUtilies.BFS always returning zero nodes (thanks Ajveach).
		- Made reverting GraphUpdateObjects work. The GraphUpdateUtilities.UpdateGraphsNoBlock was also fixed by this change.
		- Fixed compile error with monodevelop.
		- Fixed a bug which caused scanning to fail if more than one NavmeshGraph existed.
		- Fixed the lightweight local avoidance example scene which didn't work previously.
		- Fixed SimpleSmoothModifier not exposing Roundness Factor in the editor for the Curved Nonuniform mode.
		- Fixed an exception when updating RecastGraphs and using RelevantGraphSurfaces and multithreading.
		- Fixed exceptions caused by starting paths from other threads than the Unity thread.
		- Fixed an infinite loop/out of memory exception that could occur sometimes when graph updates were being done at the start of the game (I hate multithreading race conditions).
		- Fixed the Optimizations tab not working when JS Support was enabled.
		- Fixed graph updating not working on navmesh graphs (it was broken before due to a missing line of code).
		- Fixed some misspelled words in the documentation.
		- Removed some unused and/or redundant variables.
		- Fixed a case where graphs added using code might not always be configured correctly (and would throw exceptions when scanning).
		- Improved Windows Store compatibility.
		- Fixed a typo in the GridGraph which could cause compilation to fail when building for Windows Phone or Windows Store (thanks MariuszP)
		- Lots of code cleanups and comments added to various scripts.
		- Fixed some cases where MonoDevelop would pick up the wrong documention for fields since it doesn't support all features that Doxygen supports.
		- Fixed a bug which caused the points field on GraphUpdateScene to sometimes not be editable.
		- Fixed a bug which could cause RVO agents not to move if the fps was low and Interpolation and Double Buffering was used.
		- Set the execution order for RVOController and RVOSimulator to make sure that other scripts will
			get the latest position in their Update method.
		- Fixed a bug which could cause some nearest point on line methods in AstarMath to return NaN.
			This could happen when Seeker->Start End Modifier->StartPoint and EndPoint was set to Interpolate.
		- Fixed a runtime error on PS Vita.
		- Fixed an index out of range exception which could occur when scanning LayeredGridGraphs.
		- Fixed an index out of range exception which could occur when drawing gizmos for a LayeredGridGraph.
		- Fixed a bug which could cause ProduralGridMover to update the graph every frame regardless
		  of if the target moved or not (thanks Makak for finding the bug).
		- Fixed a number of warnings in Unity 5.

## 3.5.9.7 (3.6 beta 6, 2015-01-28)
## 3.5.9.6 (3.6 beta 5, 2015-01-28)
## 3.5.9.5 (3.6 beta 4, 2015-01-27)
## 3.5.9.1 (3.6 beta 3, 2014-10-14)
## 3.5.9   (3.6 beta 2, 2014-10-13)
## 3.5.8   (3.6 beta 1)
	 - See release notes for 3.6

## 3.5.2 (2013-09-01) (tiny bugfix and small feature release)
- Added isometric angle option for grid graphs to help with isometric 2D games.
- Fixed a bug with the RVOAgent class which caused the LightweightRVO example scene to not work as intended (no agents were avoiding each other).
- Fixed some documentation typos.
- Fixed some compilations errors some people were having with other compilers than Unity's.

## 3.5.1 (2014-06-15)
- Added avoidance masks to local avoidance.
		Each agent now has a layer and each agent can specify which layers it will avoid.

## 3.5 (2014-06-12)
- Added back local avoidance!!
		The new system uses a sampling based algorithm instead of a geometric one.
		The API is almost exactly the same so if you used the previous system this will be a drop in replacement.
		As for performance, it is roughly the same, maybe slightly worse in high density situations and slightly better
		in less dense situations. It can handle several thousand agents on an i7 processor.
		Obstacles are not yet supported, but they will be added in a future update.

- Binary heap switched out for a 4-ary heap.
		This improves pathfinding performances by about 5%.
- Optimized scanning of navmesh graphs (not the recast graphs)
		Large meshes should be much faster to scan now.
- Optimized BBTree (nearest node lookup for navmesh/recast graphs, pro version only)
		Nearest node queries on navmesh/recast graphs should be slightly faster now.
- Minor updates to the documentation, esp. to the GraphNode class.

## 3.4.0.7
- Vuforia test build

## 3.4.0.6
- Fixed an issue where serialization could on some machines sometimes cause an exception to get thrown.
- Fixed an issue where the recast graph would not rasterize terrains properly near the edges of it.
- Added PathUtilities.BFS.
- Added PathUtilities.GetPointsAroundPointWorld.

## 3.4.0.5
- Added offline documentation (Documentation.zip)
- Misc fixes for namespace conflicts people have been having. This should improve compatibility with other packages.
		You might need to delete the AstarPathfindingProject folder and reimport the package for everything to work.

## 3.4.0.4
- Removed RVOSimulatorEditor from the free version, it was causing compiler errors.
- Made PointGraph.nodes public.

## 3.4.0.3
- Removed Local Avoidance due to licensing issues.
		Agents will fall back to not avoiding each other.
		I am working to get the local avoidance back as soon as possible.

## 3.4.0.2
- Unity Asset Store forced me to increase version number.

## 3.4.0.1
- Fixed an ArrayIndexOutOfBounds exception which could be thrown by the ProceduralGraphMover script in the Procedural example scene if the target was moved too quickly.
- The project no longer references assets from the Standard Assets folder (the package on the Unity Asset Store did so by mistake before).

## 3.4
- Fixed a null reference exception when scanning recast graphs and rasterizing colliders.
- Removed duplicate clipper_library.dll which was causing compiler errors.
- Support for 2D Physics collision testing when using Grid Graphs.
- Better warnings when using odd settings for Grid Graphs.
- Minor cleanups.
- Queued graph updates are no longer being performed when the AstarPath object is destroyed, this just took time.
- Fixed a bug introduced in 3.3.11 which forced grid graphs to be square in Unity versions earlier than 4.3.
- Fixed a null reference in BBTree ( used by RecastGraph).
- Fixed NavmeshGraph not rebuilding BBTree on cached start (causing performance issues on larger graphs).

- Includes all changes from the beta releases below

## Beta 3.3.14 ( available for everyone! )
- All dlls are now in namespaces (e.g Pathfinding.Ionic.Zip instead of just Ionic.Zip ) to avoid conflicts with other packages.
- Most scripts are now in namespaces to avoid conflicts with other packages.
- GridNodes now support custom connections.
- Cleanups, preparing for release.
- Reverted to using an Int3 for GraphNode.position instead of an abstract Position property, the tiny memory gains were not worth it.

## Beta 3.3.13 ( 4.3 compatible only )
- Fixed an issue where deleting a NavmeshCut component would not update the underlaying graph.
- Better update checking.

## Beta 3.3.12 ( 4.3 compatible only )
- Fixed an infinite loop which could happen when scanning graphs during runtime ( not the first scan ).
- NodeLink component is now working correctly.
- Added options for optimizations to the PointGraph.
- Improved TileHandler and navmesh cutting.
- Fixed rare bug which could mess up navmeshes when using navmesh cutting.

## Beta 3.3.11 ( 4.3 compatible only )
- Fixed update checking. A bug has caused update checking not to run unless you had been running a previous version in which the bug did not exist.
		I am not sure how long this bug has been here, but potentially for a very long time.
- Added an update notification window which pops up when there is a new version of the A* Pathfinding Project.
- Lots of UI fixes for Unity 4.3
- Lots of other UI fixes and imprements.
- Fixed gravity for RichAI.
- Fixed Undo for Unity 4.3
- Added a new example scene showing a procedural environment.

## Beta 3.3.10
- Removed RecastGraph.includeOutOfBounds.
- Fixed a few bugs when updating Layered Grid Graphs causing incorrect connections to be created, and valid ones to be left out.
- Fixed a null reference bug when removing RVO agents.
- Fixed memory leaks when deserializing graphs or reloading scenes.

## Beta 3.3.9
- Added new tutorial page about recast graphs.
- Recast Graph: Fixed a bug which could cause vertical surfaces to be ignored.
- Removed support for C++ Recast.
- Fixed rare bug which could mess up navmeshes when using navmesh cutting.
- Improved TileHandler and navmesh cutting.
- GraphModifiers now take O(n) (linear) time to destroy at end of game instead of O(n^2) (quadratic).
- RecastGraph now has a toggle for using tiles or not.
- Added RelevantGraphSurface which can be used with RecastGraphs to prune away non-relevant surfaces.
- Removed RecastGraph.accurateNearestNode since it was not used anymore.
- Added RecastGraph.nearestSearchOnlyXZ.
- RecastGraph now has support for removing small areas.
- Added toggle to show or hide connections between nodes on a recast graph.
- PointNode has some graph searching methods overloaded specially. This increases performance and reduces alloacations when searching
		point graphs.
- Reduced allocations when searching on RecastGraph.
- Reduced allocations in RichAI and RichPath. Everything is pooled now, so for most requests no allocations will be done.
- Reduced allocations in general by using "yield return null" instead of "yield return 0"
- Fixed teleport for local avoidance agents. Previously moving an agent from one position to another
		could cause it to interpolate between those two positions for a brief amount of time instead of staying at the second position.

## Beta 3.3.8
- Nicer RichAI gizmo colors.
- Fixed RichAI not using raycast when no path has been calculated.

## Beta 3.3.7
- Fixed stack overflow exception in RichPath
- Fixed RichPath could sometimes generate invalid paths
- Added gizmos to RichAI

## Beta 3.3.6
- Fixed node positions being off by half a node size. GetNearest node queries on grid graphs would be slightly inexact.
- Fixed grid graph updating could get messed up when using erosion.
- ... among other things, see below

## Beta 3.3.5 and 3.3.6
- Highlights
		- Rewritten graph nodes. Nodes can now be created more easily (less overhead when creating nodes).
		- Graphs may use their custom optimized memory structure for storing nodes.
		- Performance improvements for scanning recast graphs.
		- Added a whole new AI script. RichAI (and the class RichPath for some things):
			This script is intended for navmesh based graphs and has features such as:
			- Guarantees that the character stays on the navmesh
			- Minor deviations from the path can be fixed without a path recalculation.
			- Very exact stop at endpoint (seriously, precision with something like 7 decimals).
				No more circling around the target point as with AIPath.
			- Does not use path modifiers at all (for good reasons). It has an internal funnel modifier however.
			- Simple wall avoidance to avoid too much wall hugging.
			- Basic support for off-mesh links (see example scene).
		- Improved randomness for RandomPath and FleePath, all nodes considered now have an equal chance of being selected.
		- Recast now has support for tiles. This enabled much larger worlds to be rasterized (without OutOfMemory errors) and allows for dynamic graph updates. Still slow, but much faster than
			a complete recalculation of the graph.
		- Navmesh Cutting can now be done on recast graphs. This is a kind of (relatively) cheap graph updating which punches a hole in the navmesh to make place for obstacles.
			So it only supports removing geometry, not adding it (like bridges). This update is comparitively fast, and it makes real time navmesh updating possible.
			See video: http://youtu.be/qXi5qhhGNIw.
		- Added RecastNavmeshModifier which can be attached to any GameObject to include that object in recast rasterization. It exposes more options and is also
			faster for graph updates with logarithmic lookup complexity instead of linear (good for larger worlds when doing graph updating).
		- Reintroducing special connection costs for start and end nodes.
			Before multithreading was introduced, pathfinding on navmesh graphs could recalculate
			the connection costs for the start and end nodes to take into account that the start point is not actually exactly at the start node's position
			(triangles are usually quite a larger than the player/npc/whatever).
			This didn't work with multithreading however and could mess up pathfinding, so it was removed.
			Now it has been reintroduced, working with multithreading! This means more accurate paths
			on navmeshes.
		- Added several methods to pick random points (e.g for group movement) to Pathfinding.PathUtlitilies.
		- Added RadiusModifier. A new modifier which can offset the path based on the character radius. Intended for navmesh graphs
			which are not shrinked by the character radius at start but can be used for other purposes as well.
		- Improved GraphUpdateScene gizmos. Convex gizmos are now correctly placed. It also shows a bounding box when selected (not showing this has confused a lot of people).
		- AIPath has gotten some cleanups. Among other things it now behaves correctly when disabled and then enabled again
			making it easy to pool and reuse (should that need arise).
		- Funnel modifier on grid graphs will create wider funnels for diagonals which results in nicer paths.
		- If an exception is thrown during pathfinding, the program does no longer hang at quit.
		- Split Automatic thread count into Automatic High Load and Automatic Low Load. The former one using a higher number of thread.
		- Thread count used is now shown in the editor.
		- GridGraph now supports ClosestOnNode (StartEndModifier) properly. SnapToNode gives the previous behaviour on GridGraphs (they were identical before).
		- New example scene Door2 which uses the NavmeshCut component.
- Fixes
		- Fixed spelling error in GridGraph.uniformWidthDepthGrid.
		- Erosion radius (character radius, recast graphs) could become half of what it really should be in many cases.
		- RecastGraph will not rasterize triggers.
		- Fixed recast not being able to handle multiple terrains.
		- Fixed recast generating an incorrect mesh for terrains in some cases (not the whole terrain was included).
		- Linecast on many graph types had incorrect descriptions saying that the function returns true when the line does not intersect any obstacles,
			it is actually the other way around. Descriptions corrected.
		- The list of nodes returned by a ConstantPath is now guaranteed to have no duplicates.
		- Many recast constants are now proper constants instead of static variables.
		- Fixed bug in GridNode.RemoveGridGraph which caused graphs not being cleaned up correctly. Could cause problems later on.
		- Fixed an ArgumentOutOfRange exception in ListPool class.
		- RelocateNodes on NavMeshGraph now correctly recalculates connection costs and rebuilds the internal query tree (thanks peted on the forums).
		- Much better member documentation for RVOController.
		- Exposed MaxNeighbours from IAgent to RVOController.
		- Fixed AstarData.UpdateShortcuts not being called when caching was enabled. This caused graph shortcuts such as AstarPath.astarData.gridGraph not being set
			when loaded from a cache.
		- RVOCoreSimulator/RVOSimulator now cleans up the worker threads correctly.
		- Tiled recast graphs can now be serialized.
- Changes
		- Renamed Modifier class to PathModifier to avoid naming conflicts with user scripts and other packages.
		- Cleaned up recast, put inside namespace and split into multiple files.
		- ListPool and friends are now threadsafe.
		- Removed Polygon.Dot since the Vector3 class already contains such a method.
		- The Scan functions now use callbacks for progress info instead of IEnumerators. Graphs can now output progress info as well.
		- Added Pathfinding.NavGraph.CountNodes function.
		- Removed GraphHitInfo.success field since it was not used.
		- GraphUpdateScene will now fall back to collider.bounds or renderer.bounds (depending on what is available) if no points are
			defined for the shape.
		- AstarPath.StartPath now has an option to put the path in the front of the queue to prioritize its calculation over other paths.
		- Time.fixedDeltaTime by Time.deltaTime in AIPath.RotateTowards() to work with both FixedUpdate and Update. (Thanks Pat_AfterMoon)
			You might have to configure the turn speed variable after updating since the actual rotation speed might have changed a bit depending on your settings.
		- Fixed maxNeighbourDistance not being used correctly by the RVOController script. It would stay at the default value. If you
			have had trouble getting local avoidance working on world with a large scale, this could have been the problem. (Thanks to Edgar Sun for providing a reproducible example case)
		- Graphs loaded using DeserializeGraphsAdditive will get their graphIndex variables on the nodes set to the correct values. (thanks peted for noticing the bug).
		- Fixed a null reference exception in MultiTargetPath (thanks Dave for informing me about the bug).
		- GraphUpdateScene.useWorldSpace is now false per default.
		- If no log output is disabled and we are not running in the editor, log output will be discarded as early as possible for performance.
			Even though in theory log output could be enabled between writing to internal log strings and deciding if log output should be written.
		- NavGraph.inverseMatrix is now a field, not a property (for performance). All writes to matrix should be through the SetMatrix method.
		- StartEndModifier now uses ClosestOnNode for both startPoint and endPoint by default.
- Known bugs
		- Linecasting on graphs is broken at the moment. (working for recast/navmesh graph atm. Except in very special cases)
		- RVONavmesh does not work with tiled recast graphs.



## 3.2.5.1
- Fixes
		- Pooling of paths had been accidentally disabled in AIPath.

## 3.2.5
- Changes
		- Added support for serializing dictionaries with integer keys via a Json Converter.
		- If drawGizmos is disabled on the seeker, paths will be recycled instantly.
			This will show up so that if you had a seeker with drawGizmos=false, and then enable
			drawGizmos, it will not draw gizmos until the next path request is issued.
- Fixes
		- Fixed UNITY_4_0 preprocesor directives which were indented for UNITY 4 and not only 4.0.
			Now they will be enabled for all 4.x versions of unity instead of only 4.0.
		- Fixed a path pool leak in the Seeker which could cause paths not to be released if a seeker
			was destroyed.
		- When using a non-positive maxDistance for point graphs less processing power will be used.
		- Removed unused 'recyclePaths' variable in the AIPath class.
		- NullReferenceException could occur if the Pathfinding.Node.connections array was null.
		- Fixed NullReferenceException which could occur sometimes when using a MultiTargetPath (Issue #16)
		- Changed Ctrl to Alt when recalcing path continously in the Path Types example scene to avoid
			clearing the points for the MultiTargetPath at the same time (it was also using Ctrl).
		- Fixed strange looking movement artifacts during the first few frames when using RVO and interpolation was enabled.
		- AlternativePath modifier will no longer cause underflows if penalties have been reset during the time it was active. It will now
			only log a warning message and zero the penalty.
		- Added Pathfinding.GraphUpdateObject.resetPenaltyOnPhysics (and similar in GraphUpdateScene) to force grid graphs not to reset penalties when
			updating graphs.
		- Fixed a bug which could cause pathfinding to crash if using the preprocessor directive ASTAR_NoTagPenalty.
		- Fixed a case where StartEndModifier.exactEndPoint would incorrectly be used instead of exactStartPoint.
		- AlternativePath modifier now correctly resets penalties if it is destroyed.

## 3.2.4.1
- Unity Asset Store guys complained about the wrong key image.
		I had to update the version number to submit again.

## 3.2.4
- Highlights
		- RecastGraph can now rasterize colliders as well!
		- RecastGraph can rasterize colliders added to trees on unity terrains!
		- RecastGraph will use Graphics.DrawMeshNow functions in Unity 4 instead of creating a dummy GameObject.
			This will remove the annoying "cleaning up leaked mesh object" debug message which unity would log sometimes.
			The debug mesh is now also only visible in the Scene View when the A* object is selected as that seemed
			most logical to me (don't like this? post something in the forum saying you want a toggle for it and I will implement
			one).
		- GraphUpdateObject now has a \link Pathfinding.GraphUpdateObject.updateErosion toggle \endlink specifying if erosion (on grid graphs) should be recalculated after applying the guo.
			This enables one to add walkable nodes which should have been made unwalkable by erosion.
		- Made it a bit easier (and added more correct documentation) to add custom graph types when building for iPhone with Fast But No Exceptions (see iPhone page).
- Changes
		- RecastGraph now only rasterizes enabled MeshRenderers. Previously even disabled ones would be included.
		- Renamed RecastGraph.includeTerrain to RecastGraph.rasterizeTerrain to better match other variable naming.
- Fixes
		- AIPath now resumes path calculation when the component or GameObject has been disabled and then reenabled.

## 3.2.3 (free version mostly)
- Fixes
		- A UNITY_IPHONE directive was not included in the free version. This caused compilation errors when building for iPhone.
- Changes
		- Some documentation updates

## 3.2.2
- Changes
		- Max Slope in grid graphs is now relative to the graph's up direction instead of world up (makes more sense I hope)
- Note
		- Update really too small to be an update by itself, but I was updating the build scripts I use for the project and had to upload a new version because of technical reasons.

## 3.2.1
- Fixes
		- Fixed bug which caused compiler errors on build (player, not in editor).
		- Version number was by mistake set to 3.1 instead of 3.2 in the previous version.

## 3.2
- Highlights
		- A complete Local Avoidance system is now included in the pro version!
		- Almost every allocation can now be pooled. Which means a drastically lower allocation rate (GC get's called less often).
		- Initial node penalty per graph can now be set.
			Custom graph types implementing CreateNodes must update their implementations to properly assign this value.
		- GraphUpdateScene has now many more tools and options which can be used.
		- Added Pathfinding.PathUtilities which contains some usefull functions for working with paths and nodes.
		- Added Pathfinding.Node.GetConnections to enable easy getting of all connections of a node.
			The Node.connections array does not include custom connections which for example grid graphs use.
		- Seeker.PostProcess function was added for easy postprocessing of paths calculated without a seeker.
		- AstarPath.WaitForPath. Wait (block) until a specific path has been calculated.
		- Path.WaitForPath. Wait using a coroutine until a specific path has been calculated.
		- LayeredGridGraph now has support for up to 65535 layers (theoretically, but don't try it as you would probably run out of memory)
		- Recast graph generation is now up to twice as fast!
		- Fixed some UI glitches in Unity 4.
		- Debugger component has more features and a slightly better layout.
- Fixes
		- Fixed a bug which caused the SimpleSmoothModifier with uniformSegmentLength enabled to skip points sometimes.
		- Fixed a bug where importing graphs additively which had the same GUID as a graph already loaded could cause bugs in the inspector.
		- Fixed a bug where updating a GridGraph loaded from file would throw a NullReferenceException.
		- Fixed a bug which could cause error messages for paths not to be logged
		- Fixed a number of small bugs related to updating grid graphs (especially when using erosion as well).
		- Overflows could occur in some navmesh/polygon math related functions when working with Int3s. This was because the precision of them had recently been increased.
			Further down the line this could cause incorrect answers to GetNearest queries.
			Fixed by casting to long when necessary.
		- Navmesh2.shader defined "Cull Off" twice.
		- Pathfinding threads are now background threads. This will prevent them from blocking the process to terminate if they of some reason are still alive (hopefully at least).
		- When really high penalties are applied (which could be underflowed negative penalties) a warning message is logged.
			Really high penalties (close to max uint value) can otherwise cause overflows and in some cases infinity loops because of that.
		- ClosestPointOnTriangle is now spelled correctly.
		- MineBotAI now uses Update instead of FixedUpdate.
		- Use Dark Skin option is now exposed again since it could be incorrectly set sometimes. Now you can force it to light or dark, or set it to auto.
		- Fixed recast graph bug when using multiple terrains. Previously only one terrain would be used.
		- Fixed some UI glitches in Unity 4.
- Changes
		- Removed Pathfinding.NNInfo.priority.
		- Removed Pathfinding.NearestNodePriority.
		- Conversions between NNInfo and Node are now explicit to comply with the rule of "if information might be lost: use explicit casts".
		- NNInfo is now a struct.
		- GraphHitInfo is now a struct.
		- Path.vectorPath and Path.path are now List<Vector3> and List<Node> respectively. This is done to enable pooling of resources more efficiently.
		- Added Pathfinding.Node.RecalculateConnectionCosts.
		- Moved IsPathPossible from GraphUpdateUtilities to PathUtilities.
		- Pathfinding.Path.processed was replaced with Pathfinding.Path.state. The new variable will have much more information about where
			the path is in the pathfinding pipeline.
		- <b>Paths should not be created with constructors anymore, instead use the PathPool class and then call some Setup() method</b>
		- When the AstarPath object is destroyed, calculated paths in the return queue are not returned with errors anymore, but just returned.
		- Removed depracated methods AstarPath.AddToPathPool, RecyclePath, GetFromPathPool.
- Bugs
		- C++ Version of Recast does not work on Windows.
		- GraphUpdateScene does in some cases not draw correctly positioned gizmos.
		- Starting two webplayers and closing down the first might cause the other one's pathfinding threads to crash (unity bug?) (confirmed on osx)

## 3.1.4 (iOS fixes)
- Fixes
		- More fixes for the iOS platform.
		- The "JsonFx.Json.dll" file is now correctly named.
- Changes
		- Removed unused code from DotNetZip which reduced the size of it with about 20 KB.

## 3.1.3 (free version only)
- Fixes
		- Some of the fixes which were said to have been made in 3.1.2 were actually not included in the free version of the project. Sorry about that.
		- Also includes a new JsonFx and Ionic.Zip dll. This should make it possible to build with the .Net 2.0 Subset again see:
			http://www.arongranberg.com/forums/topic/ios-problem/page/1/

## 3.1.2 (small bugfix release)
- Fixes
		- Fixed a bug which caused builds for iPhone to fail.
		- Fixed a bug which caused runtime errors on the iPhone platform.
		- Fixed a bug which caused huge lag in the editor for some users when using grid graphs.
		- ListGraphs are now correctly loaded as PointGraphs when loading data from older versions of the system.
- Changes
		- Moved JsonFx into the namespace Pathfinding.Serialization.JsonFx to avoid conflicts with users own JsonFx libraries (if they used JsonFx).

- Known bugs
		- Recast graph does not work when using static batching on any objects included.

## 3.1.1 (small bugfix release)
- Fixes
		- Fixed a bug which would cause Pathfinding.GraphUpdateUtilities.UpdateGraphsNoBlock to throw an exception when using multithreading
		- Fixed a bug which caused an error to be logged and no pathfinding working when not using multithreading in the free version of the project
		- Fixed some example scene bugs due to downgrading the project from Unity 3.5 to Unity 3.4

## 3.1
- Fixed bug which caused LayerMask fields (GridGraph inspector for example) to behave weirdly for custom layers on Unity 3.5 and up.
- The color setting "Node Connection" now actually sets the colors of the node connections when no other information should be shown using the connection colors or when no data is available.
- Put the Int3 class in a separate file.
- Casting between Int3 and Vector3 is no longer implicit. This follows the rule of "if information might be lost: use explicit casts".
- Renamed ListGraph to PointGraph. "ListGraph" has previously been used for historical reasons. PointGraph is a more suitable name.
- Graph can now have names in the editor (just click the name in the graph list)
- Graph Gizmos can now be selectively shown or hidden per graph (small "eye" icon to the right of the graph's name)
- Added GraphUpdateUtilities with many useful functions for updating graphs.
- Erosion for grid graphs can now use tags instead of walkability
- Fixed a bug where using One Way links could in some cases result in a NullReferenceException being thrown.
- Vector3 fields in the graph editors now look a bit better in Unity 3.5+. EditorGUILayout.Vector3Field didn't show the XYZ labels in a good way (no idea why)
- GridGraph.useRaycastNormal is now enabled only if the Max Slope is less than 90 degrees. Previously it was a manual setting.
- The keyboard shortcut to scan all graphs does now also work even when the graphs are not deserialized yet (which happens a lot in the editor)
- Added NodeLink script, which can be attached to GameObjects to add manual links. This system will eventually replace the links system in the A* editor.
- Added keyboard shortcuts for adding and removing links. See Menubar -> Edit -> Pathfinding
	\note Some features are restricted to Unity 3.5 and newer because of technical limitations in earlier versions (especially multi-object editing related features).


## 3.1 beta (version number 3.0.9.9 in Unity due to technical limitations of the System.Versions class)
- Multithreading is now enabled in the free version of the A* Pathfinding Project!
- Better support for graph updates called during e.g OnPostScan.
- PathID is now used as a short everywhere in the project
- G,H and penalty is now used as unsigned integers everywhere in the project instead of signed integers.
- There is now only one tag per node (if not the \#define ConfigureTagsAsMultiple is set).
- Fixed a bug which could make connections between graphs invalid when loading from file (would also log annoying error messages).
- Erosion (GridGraph) can now be used even when updating the graph during runtime.
- Fixed a bug where the GridGraph could return null from it's GetNearestForce calls which ended up later throwing a NullReferenceException.
- FunnelModifier no longer warns if any graph in the path does not implement the IFunnelGraph interface (i.e have no support for the funnel algorithm)
	and instead falls back to add node positions to the path.
- Added a new graph type : LayerGridGraph which works like a GridGraph, but has support for multiple layers of nodes (e.g multiple floors in a building).
- ScanOnStartup is now exposed in the editor.
- Separated temporary path data and connectivity data.
- Rewritten multithreading. You can now run any number of threads in parallel.
- To avoid possible infinite loops, paths are no longer returned with just an error when requested at times they should not (e.g right when destroying the pathfinding object)
- Cleaned up code in AstarPath.cs, members are now structured and many obsolete members have been removed.
- Rewritten serialization. Now uses Json for settings along with a small part hardcoded binary data (for performance and memory).
		This is a lot more stable and will be more forwards and backwards compatible.
		Data is now saved as zip files(in memory, but can be saved to file) which means you can actually edit them by hand if you want!
- Added dependency JsonFx (modified for smaller code size and better compatibility).
- Added dependency DotNetZip (reduced version and a bit modified) for zip compression.
- Graph types wanting to serialize members must add the JsonOptIn attribute to the class and JsonMember to any members to serialize (in the JsonFx.Json namespace)
- Graph types wanting to serialize a bit more data (custom), will have to override some new functions from the NavGraph class to do that instead of the old serialization functions.
- Changed from using System.Guid to a custom written Guid implementation placed in Pathfinding.Util.Guid. This was done to improve compabitility with iOS and other platforms.
	Previously it could crash when trying to create one because System.Guid was not included in the runtime.
- Renamed callback AstarPath.OnSafeNodeUpdate to AstarPath.OnSafeCallback (also added AstarPath.OnThreadSafeCallback)
- MultiTargetPath would throw NullReferenceException if no valid start node was found, fixed now.
- Binary heaps are now automatically expanded if needed, no annoying warning messages.
- Fixed a bug where grid graphs would not update the correct area (using GraphUpdateObject) if it was rotated.
- Node position precision increased from 100 steps per world unit to 1000 steps per world unit (if 1 world unit = 1m, that is mm precision).
		This also means that all costs and penalties in graphs will need to be multiplied by 10 to match the new scale.
		It also means the max range of node positions is reduced a bit... but it is still quite large (about 2 150 000 world units in either direction, that should be enough).
- If Unity 3.5 is used, the EditorGUIUtility.isProSkin field is used to toggle between light and dark skin.
- Added LayeredGridGraph which works almost the same as grid graphs, but support multiple layers of nodes.
- \note Dropped Unity 3.3 support.

	 <b>Known Bugs:</b> The C++ version of Recast does not work on Windows

## Documentation Update
- Changed from FixedUpdate to Update in the Get Started Guide. CharacterController.SimpleMove should not be called more than once per frame,
			so this might have lowered performance when using many agents, sorry about this typo.
## 3.0.9
- The List Graph's "raycast" variable is now serialized correctly, so it will be saved.
- List graphs do not generate connections from nodes to themselves anymore (yielding slightly faster searches)
- List graphs previously calculated cost values for connections which were very low (they should have been 100 times larger),
		this can have caused searches which were not very accurate on small scales since the values were rounded to the nearest integer.
- Added Pathfinding.Path.recalcStartEndCosts to specify if the start and end nodes connection costs should be recalculated when searching to reflect
		small differences between the node's position and the actual used start point. It is on by default but if you change node connection costs you might want to switch it off to get more accurate paths.
- Fixed a compile time warning in the free version from referecing obsolete variables in the project.
- Added AstarPath.threadTimeoutFrames which specifies how long the pathfinding thread will wait for new work to turn up before aborting (due to request). This variable is not exposed in the inspector yet.
- Fixed typo, either there are eight (8) or four (4) max connections per node in a GridGraph, never six (6).
- AlternativePath will no longer cause errors when using multithreading!
- Added Pathfinding.ConstantPath, a path type which finds all nodes in a specific distance (cost) from a start node.
- Added Pathfinding.FloodPath and Pathfinding.FloodPathTracer as an extreamly fast way to generate paths to a single point in for example TD games.
- Fixed a bug in MultiTargetPath which could make it extreamly slow to process. It would not use much CPU power, but it could take half a second for it to complete due to excessive yielding
- Fixed a bug in FleePath, it now returns the correct path. It had previously sometimes returned the last node searched, but which was not necessarily the best end node (though it was often close)
- Using \#defines, the pathfinder can now be better profiled (see Optimizations tab -> Profile Astar)
- Added example scene Path Types (mainly useful for A* Pro users, so I have only included it for them)
- Added many more tooltips in the editor
- Fixed a bug which would double the Y coordinate of nodes in grid graphs when loading from saved data (or caching startup)
- Graph saving to file will now work better for users of the Free version, I had forgot to include a segment of code for Grid Graphs (sorry about that)
- Some other bugfixes
## 3.0.8.2
- Fixed a critical bug which could render the A* inspector unusable on Windows due to problems with backslashes and forward slashes in paths.
## 3.0.8.1
- Fixed critical crash bug. When building, a preprocessor-directive had messed up serialization so the game would probably crash from an OutOfMemoryException.
## 3.0.8
- Graph saving to file is now exposed for users of the Free version
- Fixed a bug where penalties added using a GraphUpdateObject would be overriden if updatePhysics was turned on in the GraphUpdateObject
- Fixed a bug where list graphs could ignore some children nodes, especially common if the hierarchy was deep
- Fixed the case where empty messages would spam the log (instead of spamming somewhat meaningful messages) when path logging was set to Only Errors
- Changed the NNConstraint used as default when calling NavGraph.GetNearest from NNConstraint.Walkable to NNConstraint.None, this is now the same as the default for AstarPath.GetNearest.
- You can now set the size of the red cubes shown in place of unwalkable nodes (Settings-->Show Unwalkable Nodes-->Size)
- Dynamic search of where the EditorAssets folder is, so now you can place it anywhere in the project.
- Minor A* inspector enhancements.
- Fixed a very rare bug which could, when using multithreading cause the pathfinding thread not to start after it has been terminated due to a long delay
- Modifiers can now be enabled or disabled in the editor
- Added custom inspector for the Simple Smooth Modifier. Hopefully it will now be easier to use (or at least get the hang on which fields you should change).
- Added AIFollow.canSearch to disable or enable searching for paths due to popular request.
- Added AIFollow.canMove to disable or enable moving due to popular request.
- Changed behaviour of AIFollow.Stop, it will now set AIFollow.ccanSearch and AIFollow.ccanMove to false thus making it completely stop and stop searching for paths.
- Removed Path.customData since it is a much better solution to create a new path class which inherits from Path.
- Seeker.StartPath is now implemented with overloads instead of optional parameters to simplify usage for Javascript users
- Added Curved Nonuniform spline as a smoothing option for the Simple Smooth modifier.
- Added Pathfinding.WillBlockPath as function for checking if a GraphUpdateObject would block pathfinding between two nodes (useful in TD games).
- Unity References (GameObject's, Transforms and similar) are now serialized in another way, hopefully this will make it more stable as people have been having problems with the previous one, especially on the iPhone.
- Added shortcuts to specific types of graphs, AstarData.navmesh, AstarData.gridGraph, AstarData.listGraph
- <b>Known Bugs:</b> The C++ version of Recast does not work on Windows
## 3.0.7
- Grid Graphs can now be scaled to allow non-square nodes, good for isometric games.
- Added more options for custom links. For example individual nodes or connections can be either enabled or disabled. And penalty can be added to individual nodes
- Placed the Scan keyboard shortcut code in a different place, hopefully it will work more often now
- Disabled GUILayout in the AstarPath script for a possible small speed boost
- Some debug variables (such as AstarPath.PathsCompleted) are now only updated if the ProfileAstar define is enabled
- DynamicObstacle will now update nodes correctly when the object is destroyed
- Unwalkable nodes no longer shows when Show Graphs is not toggled
- Removed Path.multithreaded since it was not used
- Removed Path.preCallback since it was obsolate
- Added Pathfinding.XPath as a more customizable path
- Added example of how to use MultiTargetPaths to the documentation as it was seriously lacking info on that area
- The viewing mesh scaling for recast graphs is now correct also for the C# version
- The StartEndModifier now changes the path length to 2 for correct applying if a path length of 1 was passed.
- The progressbar is now removed even if an exception was thrown during scanning
- Two new example scenes have been added, one for list graphs which includes sample links, and another one for recast graphs
- Reverted back to manually setting the dark skin option, since it didn't work in all cases, however if a dark skin is detected, the user will be asked if he/she wants to enable the dark skin
- Added gizmos for the AIFollow script which shows the current waypoint and a circle around it illustrating the distance required for it to be considered "reached".
- The C# version of Recast does now use Character Radius instead of Erosion Radius (world units instead of voxels)
- Fixed an IndexOutOfRange exception which could occur when saving a graph with no nodes to file
- <b>Known Bugs:</b> The C++ version of Recast does not work on Windows
## 3.0.6
- Added support for a C++ version of Recast which means faster scanning times and more features (though almost no are available at the moment since I haven't added support for them yet).
- Removed the overload AstarData.AddGraph (string type, NavGraph graph) since it was obsolete. AstarData.AddGraph (Pathfinding.NavGraph) should be used now.
- Fixed a few bugs in the FunnelModifier which could cause it to return invalid paths
- A reference image can now be generated for the Use Texture option for Grid Graphs
- Fixed an editor bug with graphs which had no editors
- Graphs with no editors now show up in the Add New Graph list to show that they have been found, but they cannot be used
- Deleted the \a graphIndex parameter in the Pathfinding.NavGraph.Scan function. If you need to use it in your graph's Scan function, get it using Pathfinding.AstarData.GetGraphIndex
- Javascript support! At last you can use Js code with the A* Pathfinding Project! Go to A* Inspector-->Settings-->Editor-->Enable Js Support to enable it
- The Dark Skin is now automatically used if the rest of Unity uses the dark skin(hopefully)
- Fixed a bug which could cause Unity to crash when using multithreading and creating a new AstarPath object during runtime
## 3.0.5
- \link Pathfinding.PointGraph List Graphs \endlink now support UpdateGraphs. This means that they for example can be used with the DynamicObstacle script.
- List Graphs can now gather nodes based on GameObject tags instead of all nodes as childs of a specific GameObject.
- List Graphs can now search recursively for childs to the 'root' GameObject instead of just searching through the top-level children.
- Added custom area colors which can be edited in the inspector (A* inspector --> Settings --> Color Settings --> Custom Area Colors)
- Fixed a NullReference bug which could occur when loading a Unity Reference with the AstarSerializer.
- Fixed some bugs with the FleePath and RandomPath which could cause the StartEndModifier to assign the wrong endpoint to the path.
- Documentation is now more clear on what is A* Pathfinding Project Pro only features.
- Pathfinding.NNConstraint now has a variable to constrain which graphs to search (A* Pro only).\n
	  This is also available for Pathfinding.GraphUpdateObject which now have a field for an NNConstraint where it can constrain which graphs to update.
- StartPath calls on the Seeker can now take a parameter specifying which graphs to search for close nodes on (A* Pro only)
- Added the delegate AstarPath.OnAwakeSettings which is called as the first thing in the Awake function, can be used to set up settings.
- Pathfinding.UserConnection.doOverrideCost is now serialized correctly. This represents the toggle to the right of the "Cost" field when editing a link.
- Fixed some bugs with the RecastGraph when spans were partially out-of-bounds, this could generate seemingly random holes in the mesh
## 3.0.4 (only pro version affected)
- Added a Dark Skin for Unity Pro users (though it is available to Unity Free users too, even though it doesn't look very good).
	  It can be enabled through A* Inspector --> Settings --> Editor Settings --> Use Dark Skin
- Added option to include or not include out of bounds voxels (Y axis below the graph only) for Recast graphs.
## 3.0.3 (only pro version affected)
- Fixed a NullReferenceException caused by Voxelize.cs which could surface if there were MeshFilters with no Renderers on GameObjects (Only Pro version affected)
## 3.0.2
- Textures can now be used to add penalty, height or change walkability of a Grid Graph (A* Pro only)
- Slope can now be used to add penalty to nodes
- Height (Y position) can now be usd to add penalty to nodes
- Prioritized graphs can be used to enable prioritizing some graphs before others when they are overlapping
- Several bug fixes
- Included a new DynamicObstacle.cs script which can be attached to any obstacle with a collider and it will update grids around it to account for changed position
## 3.0.1
- Fixed Unity 3.3 compability
## 3.0
- Rewrote the system from scratch
- Funnel modifier
- Easier to extend the system


## x. releases are major rewrites or updates to the system.
## .x releases are quite big feature updates
## ..x releases are the most common updates, fix bugs, add some features etc.
## ...x releases are quickfixes, most common when there was a really bad bug which needed fixing ASAP.
