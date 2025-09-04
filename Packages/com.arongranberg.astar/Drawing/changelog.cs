/// <summary>
/// \page changelog Changelog
/// \order{-10}
///
/// - 1.7.9
///     - Got rid of some small GC allocations.
///
/// - 1.7.8 (2025-05-06)
///     - Fixed a minor GC allocation happening every frame when using URP.
///     - Improved performance in standalone builds when nothing is being rendered.
///     - Fixed a significant memory leak when starting unity in batch mode.
///
/// - 1.7.7 (2025-03-20)
///     - Added a new tutorial on using caching to improve performance: caching (view in online documentation for working links).
///
///     - Fixed <see cref="Draw.xz.SolidRectangle"/> would render the rectangle in the XY plane, instead of the XZ plane.
///     - Fixed an exception could be thrown when cameras were rendered without a color target.
///     - Added <see cref="PolylineWithSymbol.up"/>, to allow you to configure the orientation of the symbols. Previously it was hardcoded to Vector3.up.
///     - Added an offset parameter to <see cref="PolylineWithSymbol"/>, to allow shifting all symbols along the polyline. This is useful for animations.
///     - Fixed various minor glitches that could happen when using <see cref="PolylineWithSymbol"/>.
///
/// - 1.7.6 (2024-10-14)
///     - Fixed a compatibility issue with the high definition render pipeline, accidentally introduced in 1.7.5.
///     - Fixed gizmos were not rendered when opening prefab assets in isolation mode and the high definition render pipeline was used.
///
/// - 1.7.5 (2024-08-06)
///     - Fixed a memory leak causing references to destroyed cameras to be kept around.
///     - Fixed <see cref="Draw.xy.SolidCircle(float3,float,float,float)"/> and <see cref="Draw.xz.SolidCircle(float3,float,float,float)"/> would render the circles in the wrong location.
///     - Reduced overhead when rendering gizmos.
///     - Each component type now shows up as a scope in the Unity Profiler when rendering their gizmos.
///     - Worked around a limitation in Unity's HDRP renderer caused errors to be logged constantly when forward rendering MSAA was enabled. Depth testing will now be disabled in this case, and a single warning will be logged.
///         Unfortunately there's nothing I can do to fix the underlying issue, since it's a limitation in Unity's HDRP renderer.
///
/// - 1.7.4 (2024-02-13)
///     - Fixed compatibility with HDRP render pipeline.
///     - Improved performance when there are many cameras rendered during the same frame.
///
/// - 1.7.3 (2024-02-07)
///     - Improved performance when there are lots of components inheriting from <see cref="MonoBehaviourGizmos"/>, but they do not actually override the DrawGizmos method.
///     - Fixed compatibility with Universal Render Pipeline package version 15 and 16 (regression in 1.7.2).
///
/// - 1.7.2 (2024-02-06)
///     - Improved performance of <see cref="Draw.WireCylinder"/> and <see cref="Draw.WireCapsule"/>.
///     - Fixed a memory leak that could happen if you used a lot of custom command builders.
///     - Added an option to the project settings to increase or decrease the resolution of circles.
///     - Improved compatibility with Universal Render Pipeline package version 17.
///
/// - 1.7.1 (2023-11-14)
///     - Removed "com.unity.jobs" as a dependency, since it has been replaced by the collections package.
///     - Added support for rendering gizmos while the scene view is in wireframe mode. This is supported in Unity 2023.1 and up.
///     - Added <see cref="CommandBuilder.DashedLine"/>.
///         [Open online documentation to see images]
///     - Added <see cref="CommandBuilder.DashedPolyline"/>.
///         [Open online documentation to see images]
///
/// - 1.7.0 (2023-10-17)
///     - Added a much more ergonomic way to draw using 2D coordinates. Take a look at 2d-drawing (view in online documentation for working links) for more info.
///         [Open online documentation to see images]
///     - Deprecated several methods like <see cref="Draw.CircleXY"/> and <see cref="Draw.CircleXZ"/> to instead use the new 2D methods (Draw.xy.Circle and Draw.xz.Circle).
///         The old ones will continue to work for the time being, but they will be removed in a future update.
///     - Removed some shader code which was not supported on WebGL.
///     - Added <see cref="CommandBuilder2D.WirePill"/>
///         [Open online documentation to see images]
///     - Added <see cref="CommandBuilder.SolidTriangle"/>
///         [Open online documentation to see images]
///     - Added an overload of <see cref="Draw.Polyline"/> which takes an IReadOnlyList<T>.
///     - Added <see cref="CommandBuilder.PolylineWithSymbol"/>
///         [Open online documentation to see images]
///     - Added an overload of <see cref="CommandBuilder.WireMesh"/> that takes a NativeArray with vertices, and one with triangles.
///     - Improved look of <see cref="Draw.ArrowheadArc"/> when using a line width greater than 1.
///     - Improved performance when there are lots of objects in the scene inheriting from <see cref="MonoBehaviourGizmos"/>.
///     - Significantly reduced main-thread load when drawing in many situations by improving the Color to Color32 conversion performance.
///         Turns out Unity's built-in one is not the fastest.
///         In Burst I've cranked it up even more by using a SIMDed conversion function.
///         Common improvements are around 10% faster, but in tight loops it can be up to 50% faster.
///     - Improved performance of <see cref="Draw.WireBox"/>.
///     - Improved performance of drawing circles and arcs.
///     - Fixed name collision when both the A* Pathfinding Project and ALINE were installed in a project. This could cause the warning "There are 2 settings providers with the same name Project/ALINE." to be logged to the console.
///     - Fixed Draw.WireBox reserving the wrong amount of memory, which could lead to an exception being thrown.
///     - Fixed lines would be drawn slightly incorrectly at very shallow camera angles.
///     - Fixed a memory leak which could happen if the game was not running, and the scene view was not being re-rendered, and a script was queuing drawing commands from an editor script repeatedly.
///         Drawing commands will now get discarded after 10 seconds if no rendering happens to avoid leaking memory indefinitely.
///     - Fixed a memory leak which could happen if the game was not running in the editor, and no cameras were being rendered (e.g. on a server).
///     - Fixed shader compilation errors when deploying for PlayStation 5.
///     - Fixed circles with a normal of exactly (0,-1,0) would not be rendered.
///     - Changed <see cref="RedrawScope"/> to continue drawing items until it is disposed, instead of requiring one to call the scope.Draw method every frame.
///     - Allow a <see cref="RedrawScope"/> to be stored in unmanaged ECS components and systems.
///     - Fixed <see cref="Draw.Arrow"/> would draw a slightly narrower arrow head when the line was pointed in certain directions.
///     - Added an overload for 3x3 matrices: <see cref="Draw.WithMatrix(float3x3)"/>.
///     - Changed the behaviour for <see cref="RedrawScope"/>s. Previously they would continue drawing as long as you called RedrawScope.Draw every frame.
///         Now they will continue drawing until you dispose them. This makes them just nicer to use for most cases.
///         This is a breaking change, but since RedrawSopes have so far been a completely undocumented feature, I expect that no, or very few people, use them.
///     - Fixed compatibility with XBox.
///     - Fixed only the base camera in a camera stack would render gizmos.
///
/// - 1.6.4 (2022-09-17)
///     - <see cref="CommandBuilder.DisposeAfter"/> will now block on the given dependency before rendering the current frame by default.
///         This reduces the risk of flickering when using ECS systems as they may otherwise not have completed their work before the frame is rendered.
///         You can pass <see cref="AllowedDelay.Infinite"/> to disable this behavior for long-running jobs.
///     - Fixed recent regression causing drawing to fail in standalone builds.
///
/// - 1.6.3 (2022-09-15)
///     - Added <see cref="LabelAlignment.withPixelOffset"/>.
///     - Fixed <see cref="LabelAlignment"/> had top and bottom alignment swapped. So for example <see cref="LabelAlignment.TopLeft"/> was actually <see cref="LabelAlignment.BottomLeft"/>.
///     - Fixed shaders would sometimes cause compilation errors, especially if you changed render pipelines.
///     - Improved sharpness of <see cref="Draw.Label2D"/> and <see cref="Draw.Label3D"/> when using small font-sizes.
///         <table>
///         <tr><td>Before</td><td>After</td></tr>
///         <tr>
///         <td>
///         [Open online documentation to see images]
///         </td>
///         <td>
///         [Open online documentation to see images]
///         </td>
///         </table>
///     - Text now fades out slightly when behind or inside other objects. The fade out amount can be controlled in the project settings:
///         [Open online documentation to see images]
///     - Fixed <see cref="Draw.Label2D"/> and <see cref="Draw.Label3D"/> font sizes would be incorrect (half as large) when the camera was in orthographic mode.
///     - Fixed <see cref="Draw.WireCapsule"/> and <see cref="Draw.WireCylinder"/> would render incorrectly in certain orientations.
///
/// - 1.6.2 (2022-09-05)
///     - Fix typo causing prefabs to always be drawn in the scene view in Unity versions earlier than 2022.1, even if they were not even added to the scene.
///
/// - 1.6.1 (2022-08-31)
///     - Fix vertex buffers not getting resized correctly. This could cause exceptions to be logged sometimes. Regression in 1.6.
///
/// - 1.6 (2022-08-27)
///     - Fixed documentation and changelog URLs in the package manager.
///     - Fixed dragging a prefab into the scene view would instantiate it, but gizmos for scripts attached to it would not work.
///     - Fixed some edge cases in <see cref="Draw.WireCapsule"/> and <see cref="Draw.WireCapsule"/> which could cause NaNs and other subtle errors.
///     - Improved compatibility with WebGL as well as Intel GPUs on Mac.
///     - Added warning when using HDRP and custom passes are disabled.
///     - Improved performance of watching for destroyed objects.
///     - Reduced overhead when having lots of objects inheriting from <see cref="MonoBehaviourGizmos"/>.
///     - It's now possible to enable/disable gizmos for component types via the Unity Scene View Gizmos menu when using render pipelines in Unity 2022.1+.
///         In earlier versions of Unity, a limited API made this impossible.
///     - Made it possible to adjust the global opacity of gizmos in the Unity Project Settings.
///         [Open online documentation to see images]
///
/// - 1.5.3 (2022-05-14)
///     - Breaking changes
///         - The minimum supported Unity version is now 2020.3.
///     - The URP 2D renderer now has support for all features required by ALINE. So the warning about it not being supported has been removed.
///     - Fixed windows newlines (\\n\\r) would show up as a newline and a question mark instead of just a newline.
///     - Fixed compilation errors when using the Unity.Collections package between version 0.8 and 0.11.
///     - Improved performance in some edge cases.
///     - Fixed <see cref="Draw.SolidMesh"/> with a non-white color could affect the color of unrelated rendered lines. Thanks Chris for finding and reporting the bug.
///     - Fixed an exception could be logged when drawing circles with a zero or negative line width.
///     - Fixed various compilation errors that could show up when using newer versions of the burst package.
///
/// - 1.5.2 (2021-11-09)
///     - Fix gizmos would not show up until you selected the camera if you had just switched to the universal render pipeline.
///     - Improved performance of drawing lines by more efficiently sending the data to the shader.
///         This has the downside that shader target 4.5 is now required. I don't think this should be a big deal nowadays, but let me know if things don't work on your platform.
///         This was originally introduced in 1.5.0, but reverted in 1.5.1 due to some compatibility issues causing rendering to fail for some project configurations. I think those issues should be resolved now.
///
/// - 1.5.1 (2021-10-28)
///     - Reverted "Improved performance of drawing lines by more efficiently sending the data to the shader." from 1.5.0.
///         It turns out this caused issues for some users and could result in gizmos not showing at all.
///         I'll try to figure out a solution and bring the performance improvements back.
///
/// - 1.5 (2021-10-27)
///     - Added support FixedStrings in <see cref="Draw.Label2D(float3,FixedString32Bytes,float)"/>, which means it can be used inside burst jobs (C# managed strings cannot be used in burst jobs).
///     - Fixed a 'NativeArray has not been disposed' error message that could show up if the whole project's assets were re-imported.
///     - Added <see cref="Draw.SolidCircle"/>.
///        [Open online documentation to see images]
///     - Added <see cref="Draw.SolidCircleXZ"/>.
///        [Open online documentation to see images]
///     - Added <see cref="Draw.SolidArc"/>.
///        [Open online documentation to see images]
///     - Added <see cref="Draw.Label3D"/>
///         [Open online documentation to see images]
///     - Improved performance of <see cref="Draw.WirePlane"/> and <see cref="Draw.WireRectangle"/> by making them primitives instead of just calling <see cref="Draw.Line"/> 4 times.
///     - Improved performance in general by more efficiently re-using existing vertex buffers.
///     - Fixed some warnings related to ENABLE_UNITY_COLLECTIONS_CHECKS which burst would log when building a standalone player.
///     - Changed more functions in the <see cref="Draw"/> class to take a Unity.Mathematics.quaternion instead of a UnityEngine.Quaternion.
///         Implicit conversions exist in both directions, so there is no need to change your code.
///
/// - 1.4.3 (2021-09-04)
///     - Fixed some debug printout had been included by mistake. A "Disposing" message could sometimes show up in the console.
///
/// - 1.4.2 (2021-08-22)
///     - Reduced overhead in standalone builds if you have many objects in the scene.
///     - Fixed <see cref="Draw.WireCapsule(float3,float3,float)"/> could render incorrectly if the start and end parameters were identical.
///     - Fixed <see cref="Draw.WithDuration"/> scopes could survive until the next time the game started if no game or scene cameras were ever rendered while in edit mode.
///     - Added <see cref="Draw.SphereOutline(float3,float)"/>.
///        [Open online documentation to see images]
///     - <see cref="Draw.WireSphere(float3,float)"/> has changed to always include an outline of the sphere. This makes it a lot nicer to look at.
///        [Open online documentation to see images]
///
/// - 1.4.1 (2021-02-28)
///     - Added <see cref="CommandBuilder.DisposeAfter"/> to dispose a command builder after a job has completed.
///     - Fixed gizmos would be rendered for other objects when the scene view was in prefab isolation mode. Now they will be hidden, which matches what Unity does.
///     - Fixed a deprecation warning when unity the HDRP package version 9.0 or higher.
///     - Improved docs for <see cref="RedrawScope"/>.
///     - Fixed documentation for scopes (e.g. <see cref="Draw.WithColor"/>) would show up as missing in the online documentation.
///
/// - 1.4 (2021-01-27)
///     - Breaking changes
///         - <see cref="Draw.WireCapsule(float3,float3,float)"/> with the bottom/top parameterization was incorrect and the behavior did not match the documentation for it.
///             This method has been changed so that it now matches the documentation as this was the intended behavior all along.
///             The documentation and parameter names have also been clarified.
///     - Added <see cref="Draw.SolidRectangle(Rect)"/>.
///     - Fixed <see cref="Draw.SolidBox(float3,quaternion,float3)"/> and <see cref="Draw.WireBox(float3,quaternion,float3)"/> rendered a box that was offset by 0.5 times the size of the box.
///         This bug only applied to the overload with a rotation, not for example to <see cref="Draw.SolidBox(float3,float3)"/>.
///     - Fixed Draw.SolidMesh would always be rendered at the world origin with a white color. Now it picks up matrices and colors properly.
///     - Fixed a bug which could cause a greyed out object called 'RetainedGizmos' to appear in the scene hierarchy.
///     - Fixed some overloads of WireCylinder, WireCapsule, WireBox and SolidBox throwing errors when you tried to use them in a Burst job.
///     - Improved compatibility with some older versions of the Universal Render Pipeline.
///
/// - 1.3.1 (2020-10-10)
///     - Improved performance in standalone builds by more aggressively compiling out drawing commands that would never render anything anyway.
///     - Reduced overhead in some cases, in particular when nothing is being rendered.
///
/// - 1.3 (2020-09-12)
///     - Added support for line widths.
///         See <see cref="Draw.WithLineWidth"/>.
///         [Open online documentation to see images]
///     - Added warning message when using the Experimental URP 2D Renderer. The URP 2D renderer unfortunately does not have enough features yet
///         to be able to support ALINE. It doesn't have an extensible post processing system. The 2D renderer will be supported as soon as it is technically possible.
///     - Fixed <see cref="Draw.SolidPlane(float3,float3,float2)"/> and <see cref="Draw.WirePlane(float3,float3,float2)"/> not working for all normals.
///     - Fixed the culling bounding box for text and lines could be calculated incorrectly if text labels were used.
///         This could result in text and lines randomly disappearing when the camera was looking in particular directions.
///     - Renamed <see cref="Draw.PushPersist"/> and <see cref="Draw.PopPersist"/> to <see cref="Draw.PushDuration"/> and <see cref="Draw.PopDuration"/> for consistency with the <see cref="Draw.WithDuration"/> scope.
///         The previous names will still work, but they are marked as deprecated.
///     - Known bugs
///         - <see cref="Draw.SolidMesh(Mesh)"/> does not respect matrices and will always be drawn with the pivot at the world origin.
///
/// - 1.2.3 (2020-07-26)
///     - Fixed solid drawing not working when using VR rendering.
///     - Fixed nothing was visible when using the Universal Render Pipeline and post processing was enabled.
///         Note that ALINE will render before post processing effects when using the URP.
///         This is because as far as I can tell the Universal Render Pipeline does not expose any way to render objects
///         after post processing effects because it renders to hidden textures that custom passes cannot access.
///     - Fixed drawing sometimes not working when using the High Definition Render Pipeline.
///         In contrast to the URP, ALINE can actually render after post processing effects with the HDRP since it has a nicer API. So it does that.
///     - Known bugs
///         - <see cref="Draw.SolidMesh(Mesh)"/> does not respect matrices and will always be drawn with the pivot at the world origin.
///
/// - 1.2.2 (2020-07-11)
///     - Added <see cref="Draw.Arc(float3,float3,float3)"/>.
///         [Open online documentation to see images]
///     - Fixed drawing sometimes not working when using the Universal Render Pipeline, in particular when either HDR or anti-aliasing was enabled.
///     - Fixed drawing not working when using VR rendering.
///     - Hopefully fixed the issue that could sometimes cause "The ALINE package installation seems to be corrupt. Try reinstalling the package." to be logged when first installing
///         the package (even though the package wasn't corrupt at all).
///     - Incremented required burst package version from 1.3.0-preview.7 to 1.3.0.
///     - Fixed the offline documentation showing the wrong page instead of the get started guide.
///
/// - 1.2.1 (2020-06-21)
///     - Breaking changes
///         - Changed the size parameter of Draw.WireRect to be a float2 instead of a float3.
///             It made no sense for it to be a float3 since a rectangle is two-dimensional. The y coordinate of the parameter was never used.
///     - Added <a href="ref:Draw.WirePlane(float3,float3,float2)">Draw.WirePlane</a>.
///         [Open online documentation to see images]
///     - Added <a href="ref:Draw.SolidPlane(float3,float3,float2)">Draw.SolidPlane</a>.
///         [Open online documentation to see images]
///     - Added <a href="ref:Draw.PlaneWithNormal(float3,float3,float2)">Draw.PlaneWithNormal</a>.
///         [Open online documentation to see images]
///     - Fixed Drawing.DrawingUtilities class missed an access modifier. Now all methods are properly public and can be accessed without any issues.
///     - Fixed an error could be logged after using the WireMesh method and then exiting/entering play mode.
///     - Fixed Draw.Arrow not drawing the arrowhead properly when the arrow's direction was a multiple of (0,1,0).
///
/// - 1.2 (2020-05-22)
///     - Added page showing some advanced usages: advanced (view in online documentation for working links).
///     - Added <see cref="Drawing.Draw.WireMesh"/>.
///         [Open online documentation to see images]
///     - Added <see cref="Drawing.CommandBuilder.cameraTargets"/>.
///     - The WithDuration scope can now be used even outside of play mode. Outside of play mode it will use Time.realtimeSinceStartup to measure the duration.
///     - The WithDuration scope can now be used inside burst jobs and on different threads.
///     - Fixed WireCylinder and WireCapsule logging a warning if the normalized direction from the start to the end was exactly (1,1,1).normalized. Thanks Billy Attaway for reporting this.
///     - Fixed the documentation showing the wrong namespace for classes. It listed Pathfinding.Drawing but the correct namespace is just Drawing.
///
/// - 1.1.1 (2020-05-04)
///     - Breaking changes
///         - The vertical alignment of Label2D has changed slightly. Previously the Top and Center alignments were a bit off from the actual top/center.
///     - Fixed conflicting assembly names when used in a project that also has the A* Pathfinding Project package installed.
///     - Fixed a crash when running on iOS.
///     - Improved alignment of <see cref="Drawing.Draw.Label2D"/> when using the Top or Center alignment.
///
/// - 1.1 (2020-04-20)
///     - Added <see cref="Drawing.Draw.Label2D"/> which allows you to easily render text from your code.
///         It uses a signed distance field font renderer which allows you to render crisp text even at high resolution.
///         At very small font sizes it falls back to a regular font texture.
///         [Open online documentation to see images]
///     - Improved performance of drawing lines by about 5%.
///     - Fixed a potential crash after calling the Draw.Line(Vector3,Vector3,Color) method.
///
/// - 1.0.2 (2020-04-09)
///     - Breaking changes
///         - A few breaking changes may be done as the package matures. I strive to keep these to as few as possible, while still not sacrificing good API design.
///         - Changed the behaviour of <see cref="Drawing.Draw.Arrow(float3,float3,float3,float)"/> to use an absolute size head.
///             This behaviour is probably the desired one more often when one wants to explicitly set the size.
///             The default Draw.Arrow(float3,float3) function which does not take a size parameter continues to use a relative head size of 20% of the length of the arrow.
///             [Open online documentation to see images]
///     - Added <see cref="Drawing.Draw.ArrowRelativeSizeHead"/> which uses a relative size head.
///         [Open online documentation to see images]
///     - Added <see cref="Drawing.DrawingManager.GetBuilder"/> instead of the unnecessarily convoluted DrawingManager.instance.gizmos.GetBuilder.
///     - Added <see cref="Drawing.Draw.CatmullRom(List<Vector3>)"/> for drawing a smooth curve through a list of points.
///         [Open online documentation to see images]
///     - Made it easier to draw things that are visible in standalone games. You can now use for example Draw.ingame.WireBox(Vector3.zero, Vector3.one) instead of having to create a custom command builder.
///         See ingame (view in online documentation for working links) for more details.
///
/// - 1.0.1 (2020-04-06)
///     - Fix burst example scene not having using burst enabled (so it was much slower than it should have been).
///     - Fix text color in the SceneEditor example scene was so dark it was hard to read.
///     - Various minor documentation fixes.
///
/// - 1.0 (2020-04-05)
///     - Initial release
/// </summary>
