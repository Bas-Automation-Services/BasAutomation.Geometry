# BasAutomation.Geometry

A small, dependency-free 2D/3D computational geometry library for .NET.

It provides the plain-managed geometry primitives — no CAD kernel, no COM, no
native dependencies — that sit underneath the BasAutomation Inventor add-ins and
the *transient-BRep beam* blog series.

## Install

```
dotnet add package BasAutomation.Geometry
```

## What's in it

- **Core** — `Point2D` / `Point3D`, `Vector2D` / `Vector3D`, `Matrix2D` / `Matrix3D`,
  `Plane3D`, `Box2D` / `Box3D`, `Rectangle2D`
- **Curves** — `Line2D` / `Line3D`, `Arc2D` / `Arc3D`, `Circle2D` / `Circle3D`,
  `Ellipse2D`, behind `ICurve2D` / `ICurve3D`
- **Coordinate systems** — `CoordinateSystem2D` / `CoordinateSystem3D` transforms
- **Ray casting** — `Ray3D` and intersection utilities

Everything targets `net8.0` and has **zero package dependencies**.

## Example

```csharp
using BasAutomation.Geometry;

var start = new Point3D(0, 0, 0);
var end   = new Point3D(10, 0, 0);

var dir = (end - start).Normalized();       // Vector3D
var arc = new Arc3D(/* centre, normal, radius, angles */);

bool ccw = arc.IsCounterClockwise;
```

## Note for CAD interop

This library is intentionally **CAD-kernel independent**. When you use it to drive
Autodesk Inventor's `TransientGeometry` / `TransientBRep` API, you convert these
managed primitives into Inventor's COM types at the boundary (a thin
`Point3D → Inventor.Point` style bridge). The library itself has no Inventor
reference, so it stays usable in tests, services, and non-CAD contexts.

## License

MIT © Bas Nederveen — Bas Automation Services
