---
title: Sylves Documentation
documentType: index
_disableFooter: true
_disableBreadcrumb: true
_disableNavbar: true
_disableToc: true
_gitContribute: false
---

<div class="container">

  <div class="jumbotron">
    <h1 class="display-4">Sylves v0.1</h1>
    <p class="lead">Handles the maths and algorithms of the geometry of grids</p>
    <small class="text-muted"><a class="github-link" href="https://github.com/BorisTheBrave/sylves">View in Github</a><small>
    <hr class="my-4">
    <p>Designed to support as many grids as possible, for as many use cases</p>
    <p class="lead">
      <a class="btn btn-primary btl-lg" href="articles/index.md" role="button">Getting Started</a>
      <a class="btn btn-primary btl-lg" href="https://github.com/BorisTheBrave/sylves/releases" role="button">Download Latest</a>
      <a class="btn btn-primary btl-lg" href="https://boristhebrave.itch.io/sylves-demos" role="button">Try a demo</a>
      <a class="btn btn-primary btl-lg" href="articles/release_notes.md" role="button">Release Notes</a>
    </p>
  </div>

  <div class="row">
    <div class="col-md-8 col-md-offset-2 text-center">
      <section>
        <h2>C# Library usable from Unity or .NET</h2>
        <p class="lead"></p>
      </section>
    </div>
  </div>

  <div class="row">
    <div class="col-md-8 col-md-offset-2 text-center">
      <style>
      .carousel-indicators li {
          border-color: #BBBBBB;
          background-color: #444444;
      }
      .carousel-indicators .active {
          background-color: #999999;
      }
      .item {
          position: relative;
          height:100%;
      }
      .carousel-inner img {
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translateY(-50%) translateX(-50%);
      }
      </style>
      <div id="carousel" class="carousel slide" data-ride="carousel" data-interval="8000">
        <!-- Indicators -->
        <ol class="carousel-indicators">
          <li data-target="#carousel" data-slide-to="0" class="active"></li>
          <li data-target="#carousel" data-slide-to="1"></li>
          <li data-target="#carousel" data-slide-to="2"></li>
          <li data-target="#carousel" data-slide-to="3"></li>
          <li data-target="#carousel" data-slide-to="4"></li>
          <li data-target="#carousel" data-slide-to="5"></li>
        </ol>
        <!-- Wrapper for slides -->
        <div class="carousel-inner" role="listbox" style="width:100%; height: 320px !important;">
          <div class="item active">
            <a href="articles/grids/squaregrid.md"><img src="images/grids/square.svg"/></a>
          </div>
          <div class="item">
            <a href="https://boristhebrave.itch.io/sylves-demos"><img src="images/demo/cellpicker.gif" style="height: 320px"/></a>
          </div>
          <div class="item">
            <a href="https://boristhebrave.itch.io/sylves-demos"><img src="images/demo/pathfinding.png" style="height: 320px"/></a>
          </div>
          <div class="item">
            <a href="https://boristhebrave.itch.io/sylves-demos"><img src="images/demo/polyominoes.gif" style="height: 320px"/></a>
          </div>
          <div class="item">
            <a href="https://boristhebrave.itch.io/sylves-demos"><img src="images/demo/langton.gif" style="height: 320px"/></a>
          </div>
        </div>
        <!-- Controls -->
        <a class="left carousel-control" data-target="#carousel" role="button" data-slide="prev">
          <span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>
          <span class="sr-only">Previous</span>
        </a>
        <a class="right carousel-control" data-target="#carousel" role="button" data-slide="next">
          <span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>
          <span class="sr-only">Next</span>
        </a>
      </div>
    </div>
  </div>

  <div class="row">
    <div class="col-md-8 col-md-offset-2 text-center">
      <section>
        <h2>Features</h2>
            <h3>Sylves supports a <a href="articles/grids/index.md">wide range of different grids</a> and you can <a href="articles/creating.md">create</a> even more.</h3>
            <h3>All grids in Sylves shares a common interface, <a href="articles/concepts/index.md">IGrid</a>, so algorithms can be written once and work on any grid. </h3>
            <h3>Sylves handles many of the fiddlier grid operations, such as <a href="articles/concepts/query.md">raycasts</a> and <a href="articles/concepts/pathfinding.md">pathfinding</a>.</h3>
            <h3>Sylves comes with a sophisticated notion of direction and <a href="articles/concepts/rotation.md">rotation<a/></h3>
            <h3>Sylves supports <a href="articles/concepts/shape.md#deformation">mesh deformation</a> to squeeze meshes to fit irregular polygons.</h3>
      </section>
    </div>
  </div>
</div>