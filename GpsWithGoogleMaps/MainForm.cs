using System;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using ThinkGeo.MapSuite;
using ThinkGeo.MapSuite.Drawing;
using ThinkGeo.MapSuite.Layers;
using ThinkGeo.MapSuite.Shapes;
using ThinkGeo.MapSuite.Styles;
using ThinkGeo.MapSuite.WinForms;


namespace GPStoGoogleMapDesktop
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            winformsMap1.MapUnit = GeographyUnit.Meter;
            winformsMap1.BackgroundOverlay.BackgroundBrush = new GeoSolidBrush(GeoColor.FromArgb(255, 198, 255, 255));

            GoogleMapsOverlay googleOverlay = new GoogleMapsOverlay(); //(@"Insert your key here!", @"C:\GoogleCache");
            googleOverlay.MapType = GoogleMapsMapType.Terrain;
            winformsMap1.Overlays.Add(googleOverlay);

            // This sets the zoom levels to map to Googles.  We next make sure we snap to the zoomlevels
            winformsMap1.ZoomLevelSet = new SphericalMercatorZoomLevelSet();

            InMemoryFeatureLayer pointLayer = new InMemoryFeatureLayer();
            pointLayer.Open();
            pointLayer.Columns.Add(new FeatureSourceColumn("Text"));
            pointLayer.Close();

            //Sets the projection parameters to go from Geodetic (EPSG 4326) or decimal degrees to Google Map projection (Spherical Mercator).
            Proj4Projection proj4 = new Proj4Projection();
            proj4.InternalProjectionParametersString = Proj4Projection.GetEpsgParametersString(4326);
            proj4.ExternalProjectionParametersString = Proj4Projection.GetGoogleMapParametersString();
            //Applies the projection to the InMemoryFeatureLayer so that the point in decimal degrees (Longitude/Latitude) can be 
            //match the projection of Google Map.
            pointLayer.FeatureSource.Projection = proj4;

            //Values in Longitude and Latitude.
            double Longitude = -95.2809;
            double Latitude = 38.9543;


            //Creates the feature made of a PointShape with the Longitude and Latitude values.
            Feature GPSFeature = new Feature(new PointShape(Longitude, Latitude));

            //Format the Longitude and Latitude into a nice string as Degrees Minutes and Seconds
            string LongLat = DecimalDegreesHelper.GetDegreesMinutesSecondsStringFromDecimalDegreePoint(GPSFeature);

            //Sets the InMemoryFeatureLayer to have it displayed with Square symbol and with text.
            GPSFeature.ColumnValues.Add("Text", LongLat);
            pointLayer.InternalFeatures.Add(GPSFeature);

            pointLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = PointStyles.CreateSimplePointStyle(PointSymbolType.Square,
                                                                    GeoColor.StandardColors.Red, GeoColor.StandardColors.Black, 2, 12);
            pointLayer.ZoomLevelSet.ZoomLevel01.DefaultTextStyle = TextStyles.CreateSimpleTextStyle("Text", "Arial", 12, DrawingFontStyles.Bold,
                                                                    GeoColor.StandardColors.Black, GeoColor.StandardColors.White, 3, -10, 10);
            pointLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            LayerOverlay pointOverlay = new LayerOverlay();
            pointOverlay.Layers.Add("PointLayer", pointLayer);
            winformsMap1.Overlays.Add("PointOverlay", pointOverlay);

            //Sets the extend of the map based on the GPS point.
            proj4.Open();
            Vertex projVertex = proj4.ConvertToExternalProjection(Longitude, Latitude);
            proj4.Close();

            double extendWidth = 2300;
            double extendHeight = 1200;

            winformsMap1.CurrentExtent = new RectangleShape((projVertex.X - extendWidth), (projVertex.Y + extendHeight),
                (projVertex.X + extendWidth), (projVertex.Y - extendHeight));

            winformsMap1.Refresh();

        }


        private void winformsMap1_MouseMove(object sender, MouseEventArgs e)
        {
            //Displays the X and Y in screen coordinates.
            statusStrip1.Items["toolStripStatusLabelScreen"].Text = "X:" + e.X + " Y:" + e.Y;

            //Gets the PointShape in world coordinates from screen coordinates.
            PointShape pointShape = ExtentHelper.ToWorldCoordinate(winformsMap1.CurrentExtent, new ScreenPointF(e.X, e.Y), winformsMap1.Width, winformsMap1.Height);

            //Displays world coordinates.
            statusStrip1.Items["toolStripStatusLabelWorld"].Text = "(world) X:" + Math.Round(pointShape.X, 4) + " Y:" + Math.Round(pointShape.Y, 4);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
