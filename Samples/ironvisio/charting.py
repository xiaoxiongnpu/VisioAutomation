
import math
from geometry import *

class VerticalBarChart :

    def __init__( self) : 
        self.DataPoints = []
        self.Categories = []
        self.MaxHeight = 3.0
        self.BarWidth=1.0
        self.HorizontalDistance=0.5
        self.CategoryHeight = 0.5
        self.CategoryDistance=0.0
        self.Origin = Point(0,0)

    def Draw(self, page) :

        # Calculate Geometry
        numpoints = len(self.DataPoints)
        bottom_row_rects, top_row_rects = get_top_bottom_rects( self.Origin, self.BarWidth, self.CategoryHeight, self.MaxHeight, self.HorizontalDistance, self.CategoryDistance, numpoints)

        heights = normalize_to( (p.Value for p in self.DataPoints), self.MaxHeight)
        bar_rects = [ Rectangle.FromPointAndSize(r.LowerLeft,Size(self.BarWidth, h)) for (r,h) in zip(top_row_rects,heights) ]

        # draw bars
        barshapes = drawrects( page, bar_rects )
        settext( barshapes, [ p.Label for p in self.DataPoints ] )

        # draw category textboxes
        catshapes = drawrects( page, bottom_row_rects )
        settext( catshapes, self.Categories )



class CircleChart :

    def __init__( self) : 
        self.DataPoints = []
        self.Categories = []
        self.MaxHeight = 1.0
        self.HorizontalDistance=0.5
        self.CategoryHeight = 0.5
        self.CategoryDistance=0.0
        self.Origin = Point(0,0)

    def Draw(self, page) :
        # Calculate Geometry
        numpoints = len(self.DataPoints)
        bottom_row_rects, top_row_rects = get_top_bottom_rects( self.Origin, self.MaxHeight, self.CategoryHeight, self.MaxHeight, self.HorizontalDistance, self.CategoryDistance, numpoints)

        centers = [ r.Center for r in top_row_rects ]
        radii = normalize_areas_to_radii( (p.Value for p in self.DataPoints) , self.MaxHeight/2.0)
        circlerects = [ Rectangle.FromPointAndRadius(c,r) for (c,r) in zip(centers,radii) ]

        # draw circle
        circleshapes = drawovals(page, circlerects)
        settext( circleshapes, [p.Label for p in self.DataPoints] )

        # draw category textboxes
        catshapes = drawrects( page, bottom_row_rects )
        settext( catshapes, self.Categories )

def drawrects( page, rects ) :
    shapes = []
    for r in rects:
        shape = page.DrawRectangle(r.X0, r.Y0, r.X1, r.Y1)
        shapes.append(shape)
    return shapes

def drawovals( page, rects ) :
    shapes = []
    for r in rects:
        shape = page.DrawOval(r.X0, r.Y0, r.X1, r.Y1)
        shapes.append(shape)
    return shapes

def settext( shapes, texts) :
    for (shape,text) in zip(shapes,texts) :
        shape.Text = text

def normalize( seq ) :
    items = [v for v in seq]
    m = max( items )
    return [ float(v)/m for v in items ]

def normalize_to( seq , s) :
    items = [v for v in seq]
    m = max( items )
    return [ float(v)/m*s for v in items ]

def normalize_areas_to_radii( seq , s) :
    normalized_areas = normalize( seq )
    radii = [ math.sqrt(v/math.pi) for v in normalized_areas]
    radii = normalize_to( radii, s )
    return radii

def get_top_bottom_rects(bottom_row_origin, cellwidth, bottom_size, top_size, hdist, vdist, numpoints) :
        bottom_row_cell_size = Size(cellwidth, bottom_size)
        bottom_row_rects = get_rects_horiz( bottom_row_origin , bottom_row_cell_size , hdist, numpoints )

        top_row_origin = bottom_row_origin.AddSize( Size(0, vdist+bottom_size) )
        top_row_cell_size = Size(cellwidth, top_size)
        top_row_rects = get_rects_horiz( top_row_origin, top_row_cell_size , hdist, numpoints )

        return (bottom_row_rects, top_row_rects)
