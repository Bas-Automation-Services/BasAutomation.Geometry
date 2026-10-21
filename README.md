# BasAutomation.Geometry

A small, dependency-free 2D/3D computational geometry library for .NET.

It provides the plain-managed geometry primitives — no CAD kernel, no COM, no
native dependencies — that sit underneath the BasAutomation Inventor add-ins and
the [transient-BRep beam blog series](https://basautomationservices.com/blog).

[![NuGet](https://img.shields.io/nuget/v/BasAutomation.Geometry.svg)](https://www.nuget.org/packages/BasAutomation.Geometry)

## Install

```
dotnet add package BasAutomation.Geometry
```

## What's in it

- **Core** — `Point2D` / `Point3D`, `Vector2D` / `Vector3D`, `Matrix2D` / `Matrix3D`,
  `Plane3D`, `Box2D` / `Box3D`, `OrientedBox3D`, `Rectangle2D`, miter-plane math
- **Curves** — `Line2D` / `Line3D`, `Arc2D` / `Arc3D`, `Circle2D` / `Circle3D`,
  `Ellipse2D` / `Ellipse3D`, behind `ICurve2D` / `ICurve3D`, plus `Path3D`,
  contour offsetting, and curve ordering/intersection helpers
- **Surfaces** — swept-surface construction from profile curves: cylinder, cone
  and torus sweeps, a line-sweep classifier (radial/tangential/tilted), and an
  exact rational B-spline sweep for elliptical profiles
- **Coordinate systems** — `CoordinateSystem2D` / `CoordinateSystem3D` transforms,
  face-aligned frames, rigid-motion fitting, orientation following
- **Ray casting** — `Ray3D` and intersection utilities
- **Validation** — cross-section validation with structured issues

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
`Point3D → Inventor.Point` style bridge — shown in the blog series). The library
itself has no Inventor reference, so it stays usable in tests, services, and
non-CAD contexts.

## Building

```
dotnet build
dotnet test
```

## About this repository

This repository is a read-only public mirror, published automatically when a new
package version is released. Development happens in a private monorepo, so the
commit history here is one commit per release. Issues are welcome; for pull
requests, please open an issue first so the change can be applied upstream.

## About

Made by [Bas Automation Services](https://basautomationservices.com): Autodesk Inventor add-ins and CAD automation from Rotterdam.

## License

MIT © Bas Nederveen — Bas Automation Services
