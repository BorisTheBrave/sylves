---
title: Sylves Documentation
documentType: index
_disableFooter: true
_disableBreadcrumb: true
_disableNavbar: true
_disableToc: true
_gitContribute: false
_disableDocFxStyle: true
_disableDocFxScripts: true
---

<!-- Mostly cribbed from the free Union theme -->

<!--Google fonts-->
<link href="https://fonts.googleapis.com/css?family=Arimo:400,400i,700,700i" rel="stylesheet">

<!--vendors styles-->
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.2.1/css/all.min.css" integrity="sha512-MV7K8+y+gLIBoVD59lQIYicR65iaqukzvf/nwasF0nqhPay5w/9lJmVM2hMDcnK1OnMGCdVK+iQrJ7lzPJQd1w==" crossorigin="anonymous" referrerpolicy="no-referrer" />

<!-- Bootstrap CSS / Color Scheme -->
<link rel="stylesheet" href="styles/css/purple.css" id="theme-color">

<link rel="stylesheet"
      href="//unpkg.com/@highlightjs/cdn-assets@11.7.0/styles/base16/zenburn.min.css">

<!--navigation-->
<nav class="navbar navbar-expand-md navbar-light bg-white fixed-top sticky-navigation">
    <a class="navbar-brand mx-auto" href="index.md">
        <img src="images/logo_cropped.png" style="display: inline; width: 200px"/>
    </a>
    <button class="navbar-toggler navbar-toggler-right border-0" type="button" data-toggle="collapse" 
            data-target="#navbarCollapse" aria-controls="navbarCollapse" aria-expanded="false" aria-label="Toggle navigation">
        <span data-feather="grid"></span>
    </button>
    <div class="collapse navbar-collapse" id="navbarCollapse">
        <ul class="navbar-nav ml-auto">
            <li class="nav-item">
                <a class="nav-link page-scroll" href="#about">About</a>
            </li>
            <li class="nav-item">
                <a class="nav-link page-scroll" href="#tutorials">Tutorials</a>
            </li>
            <li class="nav-item">
                <a class="nav-link page-scroll" href="articles/index.md">Get Started</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" href="https://boristhebrave.itch.io/sylves-demos">Demo</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" href="https://github.com/BorisTheBrave/sylves"><i class="fa-brands fa-github"></i></a>
            </li>
            <li class="nav-item">
                <a class="nav-link" href="https://discord.gg/Enzu2rrJFD"><i class="fa-brands fa-discord"></i></a>
            </li>
        </ul>
        <!--
        <form class="form-inline">
            <p class="mb-0 mx-3"><a class="page-scroll font-weight-bold" href="#contact">Work with us</a></p>
        </form>
        -->
    </div>
</nav>

<!--hero header-->
<section class="pt-7 pt-md-8" id="home">
    <div class="container">
        <div class="row">
            <div class="col-md-12 mx-auto my-auto text-center">
                <h1>Sylves - Handle the maths and algorithms for the geometry of grids</h1>
                <p class="lead mt-4 mb-5">
                    An open-source C# Library usable from Unity, .NET and Godot suitable for games and procedural generation.
                </p>
                <p>
                    <a class="btn btn-primary btl-lg" href="articles/index.md" role="button">Getting Started</a>
                    <a class="btn btn-primary btl-lg" href="https://github.com/BorisTheBrave/sylves/releases" role="button">Download Latest</a>
                    <a class="btn btn-primary btl-lg" href="https://boristhebrave.itch.io/sylves-demos" role="button">Try a demo</a>
                    <a class="btn btn-primary btl-lg" href="articles/release_notes.md" role="button">Release Notes</a>
                </p>
            </div>
        </div>
        <div class="row">
            <div class="col-md-8 mx-auto my-auto">
                <p>
                    <pre><code class="lang-csharp hljs">// Create a 10x10 grid of squares of size 1.
var grid = new SquareGrid(1, new SquareBound(0, 0, 10, 10));
// List all 100 cells
var cells = grid.GetCells();
// Print the centers of each cell.
foreach(var cell in cells)
{
    Console.Log($"{cell}: {grid.GetCellCenter(cell)}");
}</code></pre>
                </p>
            </div>
        </div>
    </div>
</section>

<!-- about section -->
<section class="pb-7" id="about">
    <div class="container">
        <div class="row mt-5">
            <div class="col-md-6 order-2 order-md-1 my-md-auto">
                <h3>Wide grid support</h3>
                <p class="text-muted lead">
                    Many useful grids are supported, of all types. 2d/3d, infinite, irregular grids are all supported, with <a href="articles/creating.md">support for creating your own grids too</a>.<br/>
                    A common interface, <a href="articles/concepts/index.md">IGrid</a>, is used so you can swap between grids with ease.
                </p>
                <a href="articles/grids/index.md" class="btn btn-primary">See all grids</a>
                <a href="articles/concepts/index.md" class="btn btn-primary">Basic concepts</a>
            </div>
            <div class="col-md-6 order-1 order-md-2">
                <img src="images/all_grids.png" class="img-fluid d-block mx-auto" alt="All grids"/>
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <img src="images/demo/pathfinding.png" class="img-fluid d-block mx-auto" alt="Pathfinding"/>
            </div>
            <div class="col-md-6 my-md-auto">
                <h3>Grids as graphs</h3>
                <p class="text-muted lead">
                    Grids can be <a href="articles/concepts/topology.md">traversed cell-by-cell</a>, including support for <a href="articles/concepts/rotation.md">rotation</a> and <a href="articles/concepts/pathfinding.md">pathfinding</a>.
                </p>
            </div>
        </div>
        <div class="row mt-5">
            <div class="col-md-6 order-2 order-md-1 my-md-auto">
                <h3>Grids as geometry</h3>
                <p class="text-muted lead">
                    Grid cells have known <a href="articles/concepts/shape.md">shapes</a> and <a href="articles/concepts/position.md">positions</a>, that can easily be <a href="articles/concepts/query.md">queried</a>.<br/>
                    Tiles can be warped to fit irregular grids with <a href="articles/concepts/shape.md#deformation">deformation<a/>.
                </p>
            </div>
            <div class="col-md-6 order-1 order-md-2">
                <img src="images/demo/cellpicker.gif" class="img-fluid d-block mx-auto" alt="Cell picker"/>
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <img src="images/documentation.jpg" class="img-fluid d-block mx-auto" alt="Documentation"/>
            </div>
            <div class="col-md-6 my-md-auto">
                <h3>Fully documented</h3>
                <p class="text-muted lead">
                    Sylves comes with <a href="articles/index.md">conceptual documentation</a>, a full <a href="api/index.md">API reference</a>, <a href="articles/tutorials/index.md">tutorials</a> and even a <a href="https://github.com/BorisTheBrave/sylves-demos/">working Unity project</a> demoing the features.
                </p>
            </div>
        </div>
    </div>
</section>
<!--
<section class="pb-7" id="about">
    <div class="container">
        <hr class="my-6"/>
        <div class="row">
            <div class="col-md-6 mx-auto text-center">
                <h4 class="dot-circle font-weight-normal">We work with world's top companies to create 
                    beautiful products & apps.</h4>
            </div>
        </div>
        <div class="row mt-5">
            <div class="col-md-6 order-2 order-md-1 my-md-auto">
                <h3>Google Design</h3>
                <p class="text-muted lead">
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer id ante posuere, vestibulum mauris eget, efficitur felis. Vestibulum tincidunt sit amet odio at gravida. Cras mollis dapibus orci, in interdum odio scelerisque rhoncus.
                </p>
                <a href="#" class="btn btn-primary">View project</a>
            </div>
            <div class="col-md-6 order-1 order-md-2">
                <img src="styles/img/google-design.jpeg" class="img-fluid d-block mx-auto" alt="Google Design"/>
            </div>
            <div class="col-md-6 order-3 mx-auto border-top border-bottom mt-5 mt-md-0 py-4">
                <div class="review text-center">
                    <p class="quote">Praesent vulputate dolor velit, in condimentum odio pellentesin condimentum odio pellentesque libero.</p>
                    <div class="mt-4 d-flex flex-row align-items-center justify-content-center">
                        <img src="styles/img/client-1.jpg" class="img-review rounded-circle mr-2" alt="Client 1"/>
                        <span class="text-muted">Ryan Siddle, Google Design</span>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <img src="styles/img/facebook-messenger.jpeg" class="img-fluid d-block mx-auto" alt="Facebook Messenger"/>
            </div>
            <div class="col-md-6 my-md-auto">
                <h3>Facebook Messenger</h3>
                <p class="text-muted lead">
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer id ante posuere, vestibulum mauris eget, efficitur felis. Vestibulum tincidunt sit amet odio at gravida. Cras mollis dapibus orci, in interdum odio scelerisque rhoncus.
                </p>
                <a href="#" class="btn btn-primary">View project</a>
            </div>
            <div class="col-md-6 mx-auto border-top border-bottom mt-5 mt-md-0 py-4">
                <div class="review text-center">
                    <p class="quote">Integer id ante posuere, vestibulum mauris eget, efficitur felis.</p>
                    <div class="mt-4 d-flex flex-row align-items-center justify-content-center">
                        <img src="styles/img/client-2.jpg" class="img-review rounded-circle mr-2" alt="Client 2"/>
                        <span class="text-muted">Ameli Mao, VP Facebook</span>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-md-6 order-2 order-md-1 my-md-auto">
                <h3>Twitter Mobile</h3>
                <p class="text-muted lead">
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer id ante posuere, vestibulum mauris eget, efficitur felis. Vestibulum tincidunt sit amet odio at gravida. Cras mollis dapibus orci, in interdum odio scelerisque rhoncus.
                </p>
                <a href="#" class="btn btn-primary">View project</a>
            </div>
            <div class="col-md-6 order-1 order-md-2">
                <img src="styles/img/twitter-mobile.jpeg" class="img-fluid d-block mx-auto" alt="Twitter Mobile"/>
            </div>
            <div class="col-md-6 order-3 mx-auto border-top border-bottom mt-5 mt-md-0 py-4">
                <div class="review text-center">
                    <p class="quote">Praesent vulputate dolor velit, pellentesin condimentum odio pellentesque libero.</p>
                    <div class="mt-4 d-flex flex-row align-items-center justify-content-center">
                        <img src="styles/img/client-3.jpg" class="img-review rounded-circle mr-2" alt="Client 3"/>
                        <span class="text-muted">Kathrine Jones, Twitter</span>
                    </div>
                </div>
            </div>
        </div>
        <div class="row mt-6">
            <div class="col-md-6 mx-auto text-center">
                <h4>Want to work with us?</h4>
                <p class="lead text-muted">Ready to launch your awesome project? We'd be happy to help you.</p>
                <a href="#" class="btn btn-primary">Get in touch</a>
            </div>
        </div>
    </div>
</section>
-->

<!--services section-->
<!--
<section class="bg-light py-7" id="services">
    <div class="container">
        <div class="row">
            <div class="col-md-7 mx-auto">
                <h2 class="dot-circle">Services we offer</h2>
                <p class="text-muted lead">Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum in nisi commodo, tempus odio a, vestibulum nibh.</p>
            </div>
        </div>
        <div class="row mt-5">
            <div class="col-md-10 mx-auto">
                <div class="row card-services">
                    <div class="col-md-6 mb-3">
                        <div class="card">
                            <div class="card-body text-center">
                                <div class="icon-box border-box">
                                    <div class="icon-box-inner small-xs text-primary">
                                        <span data-feather="crop" width="30" height="30"></span>
                                    </div>
                                </div>
                                <h5 class="mt-0 mb-3">Web design</h5>
                                Nam liber tempor cum soluta nobis eleifend option congue nihil imper.
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <div class="card">
                            <div class="card-body text-center">
                                <div class="icon-box border-box">
                                    <div class="icon-box-inner small-xs text-primary">
                                        <span data-feather="monitor" width="30" height="30"></span>
                                    </div>
                                </div>
                                <h5 class="mt-0 mb-3">Web development</h5>
                                Nam liber tempor cum soluta nobis eleifend option congue nihil imper.
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <div class="card">
                            <div class="card-body text-center">
                                <div class="icon-box border-box">
                                    <div class="icon-box-inner small-xs text-primary">
                                        <span data-feather="briefcase" width="30" height="30"></span>
                                    </div>
                                </div>
                                <h5 class="mt-0 mb-3">Branding</h5>
                                Nam liber tempor cum soluta nobis eleifend option congue nihil imper.
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <div class="card">
                            <div class="card-body text-center">
                                <div class="icon-box border-box">
                                    <div class="icon-box-inner small-xs text-primary">
                                        <span data-feather="smartphone" width="30" height="30"></span>
                                    </div>
                                </div>
                                <h5 class="mt-0 mb-3">Mobile apps</h5>
                                Nam liber tempor cum soluta nobis eleifend option congue nihil imper.
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <div class="card">
                            <div class="card-body text-center">
                                <div class="icon-box border-box">
                                    <div class="icon-box-inner small-xs text-primary">
                                        <span data-feather="message-square" width="30" height="30"></span>
                                    </div>
                                </div>
                                <h5 class="mt-0 mb-3">Social media</h5>
                                Nam liber tempor cum soluta nobis eleifend option congue nihil imper.
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6 mb-3">
                        <div class="card">
                            <div class="card-body text-center">
                                <div class="icon-box border-box">
                                    <div class="icon-box-inner small-xs text-primary">
                                        <span data-feather="headphones" width="30" height="30"></span>
                                    </div>
                                </div>
                                <h5 class="mt-0 mb-3">Coaching</h5>
                                Nam liber tempor cum soluta nobis eleifend option congue nihil imper.
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6 mx-auto text-center">
                        <hr class="my-5"/>
                        <h4>Need a custom theme or app?</h4>
                        <a href="#contact" class="btn btn-primary">Work with us</a>
                    </div>
                </div>
            </div>
        </div>
    </div>
</section>
-->

<!--call to action-->
<!--
<section class="bg-hero py-8" style="background-image: url(img/parallex.jpg)">
    <div class="container">
        <div class="row">
            <div class="col-md-7 mx-auto text-center">
                <h2 class="text-white">We help the world's top companies to create amazing products.</h2>
                <p class="lead text-white my-4">Ready to launch your awesome website?</p>
                <button class="btn btn-primary">Request a free quote</button>
            </div>
        </div>
    </div>
</section>
-->

<!--process-->
<!--
<section class="py-7" id="process">
    <div class="container">
        <div class="row">
            <div class="col-md-7 mx-auto text-center">
                <h2>How we work</h2>
                <p class="lead text-muted">
                    Donec lacus enim, ullamcorper nec lectus id, ornare finibus nunc.
                    Eleifend option congue nihil imper.
                </p>
            </div>
        </div>
        <div class="row mt-5">
            <div class="col-md-7 mx-auto timeline">
                <div class="media pb-5">
                    <div class="icon-box mt-1">
                        <div class="icon-box-inner small-xs text-primary">
                            <span data-feather="disc"></span>
                        </div>
                    </div>
                    <div class="media-body">
                        <h5>Discovery</h5>
                        <p class="text-muted">Nam liber tempor cum soluta nobis eleifend option congue nihil imper.</p>
                    </div>
                </div>
                <div class="media pb-5">
                    <div class="icon-box mt-1">
                        <div class="icon-box-inner small-xs text-primary">
                            <span data-feather="copy"></span>
                        </div>
                    </div>
                    <div class="media-body">
                        <h5>UI/UX Design</h5>
                        <p class="text-muted">Nam liber tempor cum soluta nobis eleifend option congue nihil imper.</p>
                    </div>
                </div>
                <div class="media pb-5">
                    <div class="icon-box mt-1">
                        <div class="icon-box-inner small-xs text-primary">
                            <span data-feather="box"></span>
                        </div>
                    </div>
                    <div class="media-body">
                        <h5>QA & Testing</h5>
                        <p class="text-muted">Nam liber tempor cum soluta nobis eleifend option congue nihil imper.</p>
                    </div>
                </div>
                <div class="media">
                    <div class="icon-box mt-1">
                        <div class="icon-box-inner small-xs text-primary">
                            <span data-feather="server"></span>
                        </div>
                    </div>
                    <div class="media-body">
                        <h5>Deployment</h5>
                        <p class="text-muted">Nam liber tempor cum soluta nobis eleifend option congue nihil imper.</p>
                    </div>
                </div>
            </div>
        </div>
        <div class="row mt-7">
            <div class="col-md-6 mx-auto text-center">
                <h3 class="dot-circle dot-lg">90-day satisfaction guarantee.</h3>
                <p class="lead text-muted mb-4">We know you're gonna love our professional services, but let us prove it. 
                    If our service hasn't exceeded your expectations after 90 days, you'll get a full 
                    refund. Simple as that.
                </p>
                <a class="btn btn-primary page-scroll" href="#contact">Get started risk free</a>
            </div>
        </div>
    </div>
</section>
-->

<!--tutorial section-->
<section class="py-7 bg-light" id="tutorials">
    <div class="container">
        <div class="row">
            <div class="col-md-10 mx-auto">
                <h2 class="dot-circle">Tutorials</h2>
                <p class="text-muted lead">Try out some tutorials</p>
            </div>
        </div>
        <div class="row mt-5">
            <div class="col-md-6 mb-5">
                <div class="card">
                    <a href="articles/tutorials/langton.md">
                        <img class="card-img-top" src="images/demo/langton.gif" alt="Langton's Ant Tutorial">
                    </a>
                    <div class="card-body">
                        <a href="articles/tutorials/langton.md">
                            <h5 class="card-title">Langton's Ants</h5>
                            <p class="card-text">Learn how to move an entity around on a grid.</p>
                        </a>
                    </div>
                </div>
            </div>
            <div class="col-md-6 mb-5">
                <div class="card">
                    <a href="articles/tutorials/paint.md">
                        <img class="card-img-top" src="images/paint_hex_animated.gif" alt="Paint Tutorial">
                    </a>
                    <div class="card-body">
                        <a href="articles/tutorials/paint.md">
                            <h5 class="card-title">Paint program</h5>
                            <p class="card-text">How to translate mouse inputs to changes to a grid.</p>
                        </a>
                    </div>
                </div>
            </div>
            <div class="col-md-6 mb-5">
                <div class="card">
                    <a href="articles/tutorials/townscaper.md">
                         <video width="430" autoplay loop muted>
                            <source src="images/townscaper_pan.webm" type="video/webm">
                            Your browser does not support the video tag.
                            </video> 
                    </a>
                    <div class="card-body">
                        <a href="articles/tutorials/townscaper.md">
                            <h5 class="card-title">Townscaper</h5>
                            <p class="card-text">How to recreate the grids from indie hit Townscaper.</p>
                        </a>
                    </div>
                </div>
            </div>
            <!--
            <div class="col-md-6 mx-auto text-center mt-5">
                <a href="#" class="btn btn-primary">Explore more posts</a>
            </div>
            -->
        </div>
    </div>
</section>

<!--contact section-->
<!--
<section class="py-7" id="contact">
    <div class="container">
        <div class="row">
            <div class="col-md-6 mx-auto text-center">
                <h2>Want to work with us?</h2>
                <div class="divider bg-primary mx-auto"></div>
                <p class="text-muted lead">
                    Are you working on something great? We'd love to help make it happen.
                </p>
            </div>
        </div>
        <div class="row mt-5">
            <div class="col-md-8 mx-auto">
                <form>
                    <div class="row">
                        <div class="col-md-6">
                            <div class="form-group">
                                <input type="text" class="form-control" placeholder="Your name">
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-group">
                                <input type="email"  class="form-control" placeholder="Your email address">
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-group">
                                <input type="tel"  class="form-control" placeholder="Phone number">
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-group">
                                <input type="url"  class="form-control" placeholder="Your website">
                            </div>
                        </div>
                        <div class="col-12">
                            <div class="form-group">
                                <textarea rows="5"  class="form-control" placeholder="What are you looking for?"></textarea>
                            </div>
                        </div>
                    </div>
                    <div class="text-center mt-3">
                        <button class="btn btn-primary">Send your message</button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</section>
-->

<!--footer / contact-->
<!--
<footer class="py-6 bg-light">
    <div class="container">
        <div class="row">
            <div class="col-md-6 mx-auto text-center">
                <ul class="list-inline">
                    <li class="list-inline-item"><a href="#">Privacy</a></li>
                    <li class="list-inline-item"><a href="#">Terms</a></li>
                    <li class="list-inline-item"><a href="#">Affiliates</a></li>
                    <li class="list-inline-item"><a href="#">Support</a></li>
                    <li class="list-inline-item"><a href="#">Blog</a></li>
                </ul>
                <p class="text-muted small text-uppercase mt-4">
                    Follow us on social media
                </p>
                <ul class="list-inline social social-dark social-sm">
                    <li class="list-inline-item">
                        <a href=""><i class="fa fa-facebook"></i></a>
                    </li>
                    <li class="list-inline-item">
                        <a href=""><i class="fa fa-twitter"></i></a>
                    </li>
                    <li class="list-inline-item">
                        <a href=""><i class="fa fa-google-plus"></i></a>
                    </li>
                    <li class="list-inline-item">
                        <a href=""><i class="fa fa-dribbble"></i></a>
                    </li>
                </ul>
            </div>
        </div>
        <div class="row mt-5">
            <div class="col-12 text-muted text-center small-xl">
                &copy; 2019 Union - All Rights Reserved
            </div>
        </div>
    </div>
</footer>
-->

<!--scroll to top-->
<div class="scroll-top">
    <i class="fa fa-angle-up" aria-hidden="true"></i>
</div>

<!-- jQuery first, then Popper.js, then Bootstrap JS -->
<script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js"></script>
<script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/feather-icons/4.5.0/feather.min.js"></script>
<script src="styles/js/scripts.js"></script>
<script src="//unpkg.com/@highlightjs/cdn-assets@11.7.0/highlight.min.js"></script>
<script>
      hljs.highlightAll();
</script>