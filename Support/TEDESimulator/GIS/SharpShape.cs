using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TEDESimulator.GIS
{
    #region Public enums

    /// <summary>How to open a shapefile.</summary>
    public enum AccessModes
    {
        /// <summary>Open a shapefile in read-only mode.</summary>
        ReadOnly,
        /// <summary>Open a shapefile in read-write mode.</summary>
        ReadWrite
    }

    /// <summary>Possible types of attributes in the Dbf file.</summary>
    public enum AttributeTypes
    {
        /// <summary>String attribute.</summary>
        [MarshalAs(UnmanagedType.U4)]
        String = 0,
        /// <summary>Integer attribute.</summary>
        [MarshalAs(UnmanagedType.U4)]
        Integer = 1,
        /// <summary>Floating-point number attribute.</summary>
        [MarshalAs(UnmanagedType.U4)]
        Double = 2,
        /// <summary>Logical (true/false) attribute.</summary>
        [MarshalAs(UnmanagedType.U4)]
        Logical = 3,
        /// <summary>Invalid attribute.</summary>
        [MarshalAs(UnmanagedType.U4)]
        Invalid = 4,
        /// <summary>Invalid attribute.</summary>
        [MarshalAs(UnmanagedType.U4)]
        DateTime = 5
    }

    /// <summary>Possible types of shapes contained in a shapefile.</summary>
    public enum ShapefileTypes
    {
        /// <summary>Shapefile contains [mainly] points.</summary>
        [MarshalAs(UnmanagedType.U4)]
        Point = 1,
        /// <summary>Shapefile contains [mainly] arcs.</summary>
        [MarshalAs(UnmanagedType.U4)]
        Arc = 3,
        /// <summary>Shapefile contains [mainly] polygons.</summary>
        [MarshalAs(UnmanagedType.U4)]
        Polygon = 5,
        /// <summary>Shapefile contains [mainly] multi-points.</summary>
        [MarshalAs(UnmanagedType.U4)]
        MultiPoint = 8
    }

    /// <summary>Possible types of shapes.</summary>
    public enum ShapeTypes
    {
        /// <summary>Null shapes.</summary>
        [MarshalAs(UnmanagedType.U4)]
        Null = 0,
        /// <summary>2D point [pre ArcView 3.x].</summary>
        [MarshalAs(UnmanagedType.U4)]
        Point = 1,
        /// <summary>2D arc (polyline), possible in parts [pre ArcView 3.x].</summary>
        [MarshalAs(UnmanagedType.U4)]
        Arc = 3,
        /// <summary>2D polygon, possible in parts [pre ArcView 3.x].</summary>
        [MarshalAs(UnmanagedType.U4)]
        Polygon = 5,
        /// <summary>2D multi-point (related points) [pre ArcView 3.x].</summary>
        [MarshalAs(UnmanagedType.U4)]
        MultiPoint = 8,
        /// <summary>3D point [may include measure values for vertices].</summary>
        [MarshalAs(UnmanagedType.U4)]
        PointZ = 11,
        /// <summary>3D arc (polyline), possible in parts [may include measure values for vertices].</summary>
        [MarshalAs(UnmanagedType.U4)]
        ArcZ = 13,
        /// <summary>3D polygon [may include measure values for vertices].</summary>
        [MarshalAs(UnmanagedType.U4)]
        PolygonZ = 15,
        /// <summary>3D multi-point (related points) [may include measure values for vertices].</summary>
        [MarshalAs(UnmanagedType.U4)]
        MultiPointZ = 18,
        /// <summary>2D point [may include measure values for vertices].</summary>
        [MarshalAs(UnmanagedType.U4)]
        PointM = 21,
        /// <summary>2D arc (polyline), possible in parts [may include measure values for vertices].</summary>
        [MarshalAs(UnmanagedType.U4)]
        ArcM = 23,
        /// <summary>2D polygon [may include measure values for vertices].</summary>
        [MarshalAs(UnmanagedType.U4)]
        PolygonM = 25,
        /// <summary>2D multi-point (related points) [may include measure values for vertices].</summary>
        [MarshalAs(UnmanagedType.U4)]
        MultiPointM = 28,
        /// <summary>3D Complex (TIN-like) [may include measure values for vertices].</summary>
        [MarshalAs(UnmanagedType.U4)]
        MultiPatch = 31
    }

    /// <summary>Possible types of shapes parts.</summary>
    public enum PartTypes
    {
        /// <summary>Linked strip of triangles</summary>
        [MarshalAs(UnmanagedType.U4)]
        TriStrip = 0,
        /// <summary>Series of triangles that all share a single vertex, forming a fan-like structure.</summary>
        [MarshalAs(UnmanagedType.U4)]
        TriFan = 1,
        /// <summary>External ring.</summary>
        [MarshalAs(UnmanagedType.U4)]
        OuterRing = 2,
        /// <summary>Internal ring.</summary>
        [MarshalAs(UnmanagedType.U4)]
        InnerRing = 3,
        /// <summary>First ring.</summary>
        [MarshalAs(UnmanagedType.U4)]
        FirstRing = 4,
        /// <summary>Ring.</summary>
        [MarshalAs(UnmanagedType.U4)]
        Ring = 5,
    }

    #endregion


    #region Public helper classes

    /// <summary>Definition of an attribute in the Dbf file.</summary>
    public class AttributeDefinition
    {
        #region Private variables
        private String name;
        private AttributeTypes type;
        private Int32 width;
        private Int32 decimals;
        #endregion

        #region Constructors
        /// <summary>Build a new attribute definition specifying all its values.</summary>
        /// <param name="AttributeName">The name of the attribute (max 11 chars, upper-case letters).</param>
        /// <param name="AttributeType">The type of the attribute (see the enum AttributeTypes).</param>
        /// <param name="AttributeWidth">The max size of the data contained in the attribute.</param>
        /// <param name="AttributeDecimals">The max number of decimals after the comma.</param>
        public AttributeDefinition(String AttributeName, AttributeTypes AttributeType, Int32 AttributeWidth, Int32 AttributeDecimals)
        {
            this.Name = AttributeName;
            this.Type = AttributeType;
            this.Width = AttributeWidth;
            this.Decimals = AttributeDecimals;
        }
        /// <summary>Build a new attribute definition to containg floating-point numbers.</summary>
        /// <param name="AttributeName">The name of the attribute (max 11 chars, upper-case letters).</param>
        /// <param name="AttributeWidth">The max size of the data contained in the attribute.</param>
        /// <param name="AttributeDecimals">The max number of decimals after the comma.</param>
        /// <returns>Returns an AttributeDefinition representing a floating-point number attribute in the Dbf file.</returns>
        public static AttributeDefinition DoubleAttribute(String AttributeName, Int32 AttributeWidth, Int32 AttributeDecimals)
        {
            AttributeDefinition f = new AttributeDefinition(AttributeName, AttributeTypes.Double, AttributeWidth, AttributeDecimals);
            f.width = Math.Min(f.width, f.decimals + 1);
            return f;
        }
        /// <summary>Build a new attribute definition to containg integer numbers.</summary>
        /// <param name="AttributeName">The name of the attribute (max 11 chars, upper-case letters).</param>
        /// <param name="AttributeWidth">The max number of digits.</param>
        /// <returns>Returns an AttributeDefinition representing an integer number attribute in the Dbf file.</returns>
        public static AttributeDefinition IntegerAttribute(String AttributeName, Int32 AttributeWidth)
        {
            AttributeDefinition f = new AttributeDefinition(AttributeName, AttributeTypes.Integer, AttributeWidth, 0);
            f.width = Math.Max(f.width, 1);
            return f;
        }
        /// <summary>Build a new invalid attribute definition.</summary>
        /// <param name="AttributeName">The name of the attribute (max 11 chars, upper-case letters).</param>
        /// <returns>Returns an invalid AttributeDefinition.</returns>
        public static AttributeDefinition InvalidAttribute(String AttributeName)
        {
            return new AttributeDefinition(AttributeName, AttributeTypes.Invalid, 0, 0);
        }
        /// <summary>Build a new attribute definition to containg logical values.</summary>
        /// <param name="AttributeName">The name of the attribute (max 11 chars, upper-case letters).</param>
        /// <returns>Returns an AttributeDefinition representing a logical attribute in the Dbf file.</returns>
        public static AttributeDefinition LogicalAttribute(String AttributeName)
        {
            return new AttributeDefinition(AttributeName, AttributeTypes.Logical, 1, 0);
        }
        /// <summary>Build a new attribute definition to containg strings.</summary>
        /// <param name="AttributeName">The name of the attribute (max 11 chars, upper-case letters).</param>
        /// <param name="AttributeWidth">The max number of characters.</param>
        /// <returns>Returns an AttributeDefinition representing a string attribute in the Dbf file.</returns>
        public static AttributeDefinition StringAttribute(String AttributeName, Int32 AttributeWidth)
        {
            return new AttributeDefinition(AttributeName, AttributeTypes.String, Math.Max(1, AttributeWidth), 0);
        }
        #endregion

        #region Public properties
        /// <summary>The attribute name.</summary>
        public String Name
        {
            get
            { return name; }
            set
            {
                name = String.IsNullOrEmpty(value) ? "" : value.ToUpper();
                if (name.Length > 11)
                    name = name.Substring(0, 11);
            }
        }
        /// <summary>The attribute type.</summary>
        public AttributeTypes Type
        {
            get
            { return type; }
            set
            { type = value; }
        }
        /// <summary>The maximum with of the data contained in the field.</summary>
        public Int32 Width
        {
            get
            { return width; }
            set
            { width = Math.Max(0, value); }
        }
        /// <summary>The maximum with of the data after the decimal sign.</summary>

        public Int32 Decimals
        {
            get
            { return decimals; }
            set
            { decimals = Math.Max(0, value); }
        }
        /// <summary>Builds a string representation of this field.</summary>
        /// <returns>The string representation of this field (ie the field name).</returns>
        public override String ToString()
        {
            return Name;
        }
        #endregion
    }

    /// <summary>An area.</summary>
    public class Area
    {
        /// <summary>The left (west) coordinate of the area.</summary>
        public Double Left;
        /// <summary>The right (east) coordinate of the area.</summary>
        public Double Right;
        /// <summary>The bottom (south) coordinate of the area.</summary>
        public Double Bottom;
        /// <summary>The top (north) coordinate of the area.</summary>
        public Double Top;
        /// <summary>The minimum height of the area.</summary>
        public Double MinZ;
        /// <summary>The maximum height of the area.</summary>
        public Double MaxZ;
        /// <summary>The minimum measure of the area.</summary>
        public Double MinM;
        /// <summary>The maximum measure of the area.</summary>
        public Double MaxM;
        /// <summary>The width (east-west) of the area.</summary>
        public Double Width
        {
            get
            { return Right - Left; }
            set
            { Right = Left + value; }
        }
        /// <summary>The height (north-south) of the area.</summary>
        public Double Height
        {
            get
            { return Top - Bottom; }
            set
            { Top = Bottom + value; }
        }
        /// <summary>Builds a new area starting from its boundary.</summary>
        /// <param name="AreaLeft">The left (west) coordinate of the area.</param>
        /// <param name="AreaRight">The right (east) coordinate of the area.</param>
        /// <param name="AreaBottom">The bottom (south) coordinate of the area.</param>
        /// <param name="AreaTop">The top (north) coordinate of the area.</param>
        public Area(Double AreaLeft, Double AreaRight, Double AreaBottom, Double AreaTop)
        {
            Left = AreaLeft;
            Right = AreaRight;
            Bottom = AreaBottom;
            Top = AreaTop;
            MaxM = MinM = MaxZ = MinZ = 0D;
        }
        /// <summary>Builds a new area starting from its boundary.</summary>
        /// <param name="AreaLeft">The left (west) coordinate of the area.</param>
        /// <param name="AreaRight">The right (east) coordinate of the area.</param>
        /// <param name="AreaBottom">The bottom (south) coordinate of the area.</param>
        /// <param name="AreaTop">The top (north) coordinate of the area.</param>
        /// <param name="AreaMinZ">The minimum height of the area.</param>
        /// <param name="AreaMaxZ">The maximum height of the area.</param>
        public Area(Double AreaLeft, Double AreaRight, Double AreaBottom, Double AreaTop, Double AreaMinZ, Double AreaMaxZ)
        {
            Left = AreaLeft;
            Right = AreaRight;
            Bottom = AreaBottom;
            Top = AreaTop;
            MinZ = AreaMinZ;
            MaxZ = AreaMaxZ;
            MaxM = MinM = 0D;
        }
        /// <summary>Builds a new area starting from its boundary.</summary>
        /// <param name="AreaLeft">The left (west) coordinate of the area.</param>
        /// <param name="AreaRight">The right (east) coordinate of the area.</param>
        /// <param name="AreaBottom">The bottom (south) coordinate of the area.</param>
        /// <param name="AreaTop">The top (north) coordinate of the area.</param>
        /// <param name="AreaMinZ">The minimum height of the area.</param>
        /// <param name="AreaMaxZ">The maximum height of the area.</param>
        /// <param name="AreaMinM">The minimum measure of the area.</param>
        /// <param name="AreaMaxM">The maximum measure of the area.</param>
        public Area(Double AreaLeft, Double AreaRight, Double AreaBottom, Double AreaTop, Double AreaMinZ, Double AreaMaxZ, Double AreaMinM, Double AreaMaxM)
        {
            Left = AreaLeft;
            Right = AreaRight;
            Bottom = AreaBottom;
            Top = AreaTop;
            MinZ = AreaMinZ;
            MaxZ = AreaMaxZ;
            MinM = AreaMinM;
            MaxM = AreaMaxM;
        }
        /// <summary>Extends this area so that it contains also the specified area.</summary>
        /// <param name="area">The area that must be contained in this area (if null nothing happens).</param>
        public void ExpandTo(Area area)
        {
            if (area == null)
                return;
            Left = Math.Min(Left, area.Left);
            Right = Math.Max(Right, area.Right);
            Bottom = Math.Min(Bottom, area.Bottom);
            Top = Math.Max(Top, area.Top);
            MinZ = Math.Min(MinZ, area.MinZ);
            MaxZ = Math.Max(MinZ, area.MaxZ);
            MinM = Math.Min(MinM, area.MinM);
            MaxM = Math.Max(MinM, area.MaxM);
        }
    }

    /// <summary>A vertex of the shape.</summary>
    public class Vertex
    {
        #region Public properties
        /// <summary>The X coordinate of the vertex.</summary>
        public Double X;
        /// <summary>The Y coordinate of the vertex.</summary>
        public Double Y;
        /// <summary>The Z coordinate of the vertex.</summary>
        public Double Z = 0D;
        /// <summary>The measure of the vertex.</summary>
        public Double M = 0D;
        #endregion

        #region Public constructors
        /// <summary>Build a new 2D vertex.</summary>
        /// <param name="VertexX">The X coordinate of the vertex.</param>
        /// <param name="VertexY">The Y coordinate of the vertex.</param>
        public Vertex(Double VertexX, Double VertexY)
        {
            X = VertexX;
            Y = VertexY;
        }
        /// <summary>Build a new 3D vertex.</summary>
        /// <param name="VertexX">The X coordinate of the vertex.</param>
        /// <param name="VertexY">The Y coordinate of the vertex.</param>
        /// <param name="VertexZ">The Z coordinate of the vertex.</param>
        public Vertex(Double VertexX, Double VertexY, Double VertexZ)
        {
            X = VertexX;
            Y = VertexY;
            Z = VertexZ;
        }
        /// <summary>Build a new 3D vertex with measure.</summary>
        /// <param name="VertexX">The X coordinate of the vertex.</param>
        /// <param name="VertexY">The Y coordinate of the vertex.</param>
        /// <param name="VertexZ">The Z coordinate of the vertex.</param>
        /// <param name="VertexM">The measure of the vertex.</param>
        public Vertex(Double VertexX, Double VertexY, Double VertexZ, Double VertexM)
        {
            X = VertexX;
            Y = VertexY;
            Z = VertexZ;
            M = VertexM;
        }
        #endregion
    }

    /// <summary>A list of vertices.</summary>
    public class VertexList : List<Vertex>
    {
        /// <summary>Initializes a new instance of a list of vertices.</summary>
        public VertexList()
            : base()
        { }
        /// <summary>Builds a new list of vertices starting from an existing one.</summary>
        /// <param name="InitialVertices">The initial list of vertices</param>
        public VertexList(List<Vertex> InitialVertices)
            : base()
        {
            AddRange(InitialVertices);
        }
        /// <summary>Adds a vertex to the end of the vertex list.</summary>
        /// <param name="item">The Vertex to be added at the end of the vertex list (null vertices won't be added).</param>
        public new void Add(Vertex item)
        {
            if (item != null)
                base.Add(item);
        }
        /// <summary>Adds the specified vertices to the end of the vertex list.</summary>
        /// <param name="collection">The vertices to be added at the end of the vertex list (null vertices won't be added).</param>
        public new void AddRange(IEnumerable<Vertex> collection)
        {
            if (collection == null)
                return;
            foreach (Vertex v in collection)
                if (v != null)
                    base.Add(v);
        }
        /// <summary>Inserts the specified vertex at the specified position.</summary>
        /// <param name="index">Zero-based index where the vertex will be inserted.</param>
        /// <param name="item">The vertex to add (null vertices won't be added).</param>
        public new void Insert(Int32 index, Vertex item)
        {
            if (item != null)
                base.Insert(index, item);
        }
        /// <summary>Inserts the specified vertices at the specified position.</summary>
        /// <param name="index">Zero-based index where the vertex will be inserted.</param>
        /// <param name="collection">The vertices to be inserted into the vertex list (null vertices won't be added).</param>
        public new void InsertRange(Int32 index, IEnumerable<Vertex> collection)
        {
            if (collection != null)
            {
                foreach (Vertex v in collection)
                {
                    if (v != null)
                        base.Insert(index++, v);
                }
            }
        }
        /// <summary>The minimum area fully containing all the vertices (null if no vertex is defined).</summary>
        public Area Limits
        {
            get
            {
                Area a = null;
                foreach (Vertex v in this)
                {
                    if (a == null)
                    {
                        a = new Area(v.X, v.X, v.Y, v.Y);
                    }
                    else
                    {
                        a.Left = Math.Min(a.Left, v.X);
                        a.Right = Math.Max(a.Right, v.X);
                        a.Bottom = Math.Min(a.Bottom, v.Y);
                        a.Top = Math.Max(a.Top, v.Y);
                    }
                }
                return a;
            }
        }
    }

    #endregion


    #region Main classes

    /// <summary>The class representing a whole shape file.</summary>
    public class ShapeFile : IDisposable, IEnumerable
    {
        #region ShapeLib imports
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern UInt32 SHPOpen(String FullFileName, String Access);

        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern UInt32 SHPCreate(String FullFileName, ShapefileTypes Type);

        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern void SHPClose(UInt32 ShpHandle);

        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern void SHPGetInfo(UInt32 ShpHandle, out Int32 Entities, out ShapefileTypes ShapeType, IntPtr pMinBound, IntPtr pMaxBound);

        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern UInt32 DBFOpen(String DbfFile, String Access);

        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern UInt32 DBFCreate(String DbfFile);

        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern void DBFClose(UInt32 DbfHandle);

        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern Int32 DBFAddField(UInt32 DbfHandle, String FieldName, AttributeTypes DBFFieldType, Int32 Width, Int32 Decimals);

        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern Int32 DBFGetFieldCount(UInt32 DbfHandle);

        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern AttributeTypes DBFGetFieldInfo(UInt32 DbfHandle, Int32 FieldIndex, IntPtr FieldName, out Int32 Width, out Int32 Decimals);
        #endregion

        #region Private variables
        private String baseFileName;
        private AccessModes accessMode;
        private UInt32 shxHandle = 0;
        private UInt32 dbfHandle = 0;
        private ShapefileTypes shapefileType;
        private List<AttributeDefinition> attributes = null;
        #endregion

        #region Public properties
        /// <summary>Returns the way she shapefile has been opened.</summary>
        public AccessModes AccessMode
        {
            get
            { return accessMode; }
        }
        /// <summary>The default type of shapes contained in the shapefile.</summary>
        public ShapefileTypes ShapefileType
        {
            get
            {
                return shapefileType;
            }
        }
        /// <summary>The handle to the shp/shx files.</summary>
        public UInt32 ShxHandle
        {
            get
            { return shxHandle; }
        }
        /// <summary>The handle to the dbf file.</summary>
        public UInt32 DbfHandle
        {
            get
            { return dbfHandle; }
        }
        /// <summary>The number of shapes in the shapefile.</summary>
        public Int32 NumShapes
        {
            get
            {
                ShapefileTypes foo;
                Int32 numShapes;
                SHPGetInfo(shxHandle, out numShapes, out foo, IntPtr.Zero, IntPtr.Zero);
                return numShapes;
            }
        }
        /// <summary>The list of the attributes defined for this shapefile.</summary>
        public AttributeDefinition[] Attributes
        {
            get
            {
                return attributes.ToArray();
            }
        }
        #endregion

        #region Private methods
        private static String GetBaseFileName(String FullFileName, out String ErrorDescription, out List<String> ExistingFiles, out List<String> NotExistingFiles)
        {
            ExistingFiles = new List<String>();
            NotExistingFiles = new List<String>();
            if (!String.IsNullOrEmpty(FullFileName))
                FullFileName = FullFileName.Trim();
            if (String.IsNullOrEmpty(FullFileName))
            {
                ErrorDescription = "Nome del file shape non specificato";
                return null;
            }
            String FolderName = "";
            String FileName = "";
            Int32 p;
            p = Math.Max(FullFileName.LastIndexOf(Path.DirectorySeparatorChar), FullFileName.LastIndexOf(Path.AltDirectorySeparatorChar));
            if ((p >= 0) && (p == (FullFileName.Length - 1)))
            {
                ErrorDescription = "Nome del file non valido";
                return null;
            }
            if (p < 0)
            {
                FolderName = Directory.GetCurrentDirectory();
                FileName = FullFileName;
            }
            else
            {
                try
                {
                    FolderName = Path.GetDirectoryName(FullFileName);
                    FileName = Path.GetFileName(FullFileName);
                    if (String.IsNullOrEmpty(FolderName) || String.IsNullOrEmpty(FileName))
                    {
                        ErrorDescription = "Nome del file non valido";
                        return null;
                    }
                }
                catch
                {
                    ErrorDescription = "Nome del file non valido";
                    return null;
                }
            }
            while (FileName.EndsWith("."))
            {
                if (FileName.Equals("."))
                {
                    ErrorDescription = "Nome del file non valido";
                    return null;
                }
                FileName = FileName.Substring(0, FileName.Length - 1);
            }
            if (FileName.Equals(".shp", StringComparison.OrdinalIgnoreCase) || FileName.Equals(".shx", StringComparison.OrdinalIgnoreCase) || FileName.Equals(".dbf", StringComparison.OrdinalIgnoreCase))
            {
                ErrorDescription = "Nome del file non valido";
                return null;
            }
            String Ext;
            try
            {
                if ((Ext = Path.GetExtension(FileName)) == null)
                    Ext = "";
                else
                    Ext = Ext.ToLower();
            }
            catch
            {
                ErrorDescription = "Nome del file non valido";
                return null;
            }
            switch (Ext)
            {
                case ".shp":
                case ".shx":
                case ".dbx":
                    FileName = Path.GetFileNameWithoutExtension(FullFileName);
                    try
                    {
                        switch (Path.GetExtension(FileName).ToLower())
                        {
                            case ".shp":
                            case ".shx":
                            case ".dbx":
                            case ".":
                                ErrorDescription = "Nome del file non valido";
                                return null;
                        }
                    }
                    catch
                    {
                    }
                    break;
            }
            String R;
            try
            {
                String s;
                R = Path.Combine(FolderName, FileName);
                if (File.Exists(s = R + ".shp"))
                    ExistingFiles.Add(s);
                else
                    NotExistingFiles.Add(s);
                if (File.Exists(s = R + ".shx"))
                    ExistingFiles.Add(s);
                else
                    NotExistingFiles.Add(s);
                if (File.Exists(s = R + ".dbf"))
                    ExistingFiles.Add(s);
                else
                    NotExistingFiles.Add(s);
            }
            catch (Exception x)
            {
                ErrorDescription = ((x.InnerException == null) || String.IsNullOrEmpty(x.InnerException.Message)) ? x.Message : x.InnerException.Message;
                return null;
            }
            ErrorDescription = null;
            return R;
        }
        #endregion

        #region Public methods

        /// <summary>Returns the base file name of a shapefile (ie the filename without .shp, .shx or .dbf).</summary>
        /// <param name="FullFileName">A path to anyone of the three files of the shapefile, of the file without any extension.</param>
        /// <returns>The base file name of the shapefile, or null in case of error.</returns>
        public static String GetBaseFileName(String FullFileName)
        {
            String foo;
            return GetBaseFileName(FullFileName, out foo);
        }
        /// <summary>Returns the base file name of a shapefile (ie the filename without .shp, .shx or .dbf).</summary>
        /// <param name="FullFileName">A path to anyone of the three files of the shapefile, of the file without any extension.</param>
        /// <param name="ErrorDescription">The description of the error (or null in case the function succeded).</param>
        /// <returns>The base file name of the shapefile, or null in case of error.</returns>
        /// <returns></returns>
        public static String GetBaseFileName(String FullFileName, out String ErrorDescription)
        {
            List<String> foo1, foo2;
            return GetBaseFileName(FullFileName, out ErrorDescription, out foo1, out foo2);
        }

        /// <summary>Open a shapefile in read/write mode.</summary>
        /// <param name="FullFileName">The path to the shapefile.</param>
        /// <returns>Returns she ShapeFile object representing the file, or null in case of error.</returns>
        public static ShapeFile Open(String FullFileName)
        {
            String foo;
            return Open(FullFileName, AccessModes.ReadWrite, out foo);
        }
        /// <summary>Open a shapefile in read/write mode.</summary>
        /// <param name="FullFileName">The path to the shapefile.</param>
        /// <param name="ErrorDescription">null in case of success, a description of the error in case the function fails.</param>
        /// <returns>Returns she ShapeFile object representing the file, or null in case of error.</returns>
        public static ShapeFile Open(String FullFileName, out String ErrorDescription)
        {
            return Open(FullFileName, AccessModes.ReadWrite, out ErrorDescription);
        }
        /// <summary>Open a shapefile.</summary>
        /// <param name="FullFileName">The path to the shapefile.</param>
        /// <param name="AccessMode">How to open the shapefile (in read-only or read/write mode).</param>
        /// <returns>Returns she ShapeFile object representing the file, or null in case of error.</returns>
        public static ShapeFile Open(String FullFileName, AccessModes AccessMode)
        {
            String foo;
            return Open(FullFileName, AccessMode, out foo);
        }
        /// <summary>Open a shapefile.</summary>
        /// <param name="FullFileName">The path to the shapefile.</param>
        /// <param name="AccessMode">How to open the shapefile (in read-only or read/write mode).</param>
        /// <param name="ErrorDescription">null in case of success, a description of the error in case the function fails.</param>
        /// <returns>Returns she ShapeFile object representing the file, or null in case of error.</returns>
        public static ShapeFile Open(String FullFileName, AccessModes AccessMode, out String ErrorDescription)
        {
            List<String> lExistantFiles, lNotExistantFiles;
            String BaseFileName = GetBaseFileName(FullFileName, out ErrorDescription, out lExistantFiles, out lNotExistantFiles);
            if (ErrorDescription != null)
                return null;
            switch (lNotExistantFiles.Count)
            {
                case 0:
                    break;
                case 1:
                    ErrorDescription = "Unable to find the file " + lNotExistantFiles[0];
                    return null;
                default:
                    ErrorDescription = "Unable to find the following files:";
                    foreach (String s in lNotExistantFiles)
                        ErrorDescription += Environment.NewLine + s;
                    return null;
            }
            ShapeFile sf = null;
            IntPtr pGlobalMemory = IntPtr.Zero;
            ErrorDescription = null;
            try
            {
                sf = new ShapeFile();
                sf.baseFileName = BaseFileName;
                sf.accessMode = AccessMode;
                sf.shxHandle = SHPOpen(sf.baseFileName, (sf.accessMode == AccessModes.ReadOnly) ? "rb" : "rb+");
                if (sf.shxHandle == 0)
                    throw new ApplicationException("Unable to open the shapefile .shp (or .shx) associated to " + sf.baseFileName);
                sf.dbfHandle = DBFOpen(sf.baseFileName, (sf.accessMode == AccessModes.ReadOnly) ? "rb" : "rb+");
                if (sf.dbfHandle == 0)
                    throw new ApplicationException("Unable to open the .dbf file associated to " + sf.baseFileName);
                Int32 foo;
                SHPGetInfo(sf.shxHandle, out foo, out sf.shapefileType, IntPtr.Zero, IntPtr.Zero);
                Int32 nAttributes = DBFGetFieldCount(sf.dbfHandle);
                if (nAttributes < 0)
                    throw new ApplicationException("An error occurred while determining the number of attributes.");
                pGlobalMemory = Marshal.StringToHGlobalAnsi(new String('\x00', 20));
                if (pGlobalMemory == IntPtr.Zero)
                    throw new OutOfMemoryException();
                sf.attributes = new List<AttributeDefinition>();
                for (Int32 iAttribute = 0; iAttribute < nAttributes; iAttribute++)
                {
                    Int32 aWidth, aDecimals;
                    AttributeTypes aType = DBFGetFieldInfo(sf.dbfHandle, iAttribute, pGlobalMemory, out aWidth, out aDecimals);
                    switch (aType)
                    {
                        case AttributeTypes.Double:
                        case AttributeTypes.Integer:
                        case AttributeTypes.Logical:
                        case AttributeTypes.String:
                            sf.attributes.Add(new AttributeDefinition(Marshal.PtrToStringAnsi(pGlobalMemory), aType, aWidth, aDecimals));
                            break;
                        default:
                            sf.attributes.Add(new AttributeDefinition(Marshal.PtrToStringAnsi(pGlobalMemory), aType, aWidth, aDecimals));
                            break;
                            //  throw new ApplicationException("An error occurred while retrieving the definition of the field at position " + iAttribute.ToString() + ".");
                    }
                }
                return sf;
            }
            catch (Exception x)
            {
                if (pGlobalMemory != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pGlobalMemory);
                    pGlobalMemory = IntPtr.Zero;
                }
                if (sf != null)
                {
                    sf.Dispose();
                    sf = null;
                }
                ErrorDescription = ((x.InnerException == null) || String.IsNullOrEmpty(x.InnerException.Message)) ? x.Message : x.InnerException.Message;
                return null;
            }
        }

        /// <summary>Creates a new shape file and opens it (no attribute will be created).</summary>
        /// <param name="FullFileName">The path to the shapefile to create.</param>
        /// <param name="DefaultShapeType">The default type of shapes contained in the shapefile.</param>
        /// <returns>Returns she ShapeFile object representing the file, or null in case of error.</returns>
        /// <returns></returns>
        public static ShapeFile Create(String FullFileName, ShapefileTypes DefaultShapeType)
        {
            String foo;
            return Create(FullFileName, DefaultShapeType, null, out foo);
        }
        /// <summary>Creates a new shape file and opens it.</summary>
        /// <param name="FullFileName">The path to the shapefile to create.</param>
        /// <param name="DefaultShapeType">The default type of shapes contained in the shapefile.</param>
        /// <param name="Attributes">A list of attributes to create.</param>
        /// <returns>Returns she ShapeFile object representing the file, or null in case of error.</returns>
        public static ShapeFile Create(String FullFileName, ShapefileTypes DefaultShapeType, AttributeDefinition[] Attributes)
        {
            String foo;
            return Create(FullFileName, DefaultShapeType, Attributes, out foo);
        }
        /// <summary>Creates a new shape file and opens it (no attribute will be created).</summary>
        /// <param name="FullFileName">The path to the shapefile to create.</param>
        /// <param name="DefaultShapeType">The default type of shapes contained in the shapefile.</param>
        /// <param name="ErrorDescription">null in case of success, or the description of the error in case the function fails.</param>
        /// <returns>Returns she ShapeFile object representing the file, or null in case of error.</returns>
        public static ShapeFile Create(String FullFileName, ShapefileTypes DefaultShapeType, out String ErrorDescription)
        {
            return Create(FullFileName, DefaultShapeType, null, out ErrorDescription);
        }
        /// <summary>Creates a new shape file and opens it.</summary>
        /// <param name="FullFileName">The path to the shapefile to create.</param>
        /// <param name="DefaultShapeType">The default type of shapes contained in the shapefile.</param>
        /// <param name="Attributes">A list of attributes to create.</param>
        /// <param name="ErrorDescription">null in case of success, or the description of the error in case the function fails.</param>
        /// <returns>Returns she ShapeFile object representing the file, or null in case of error.</returns>
        public static ShapeFile Create(String FullFileName, ShapefileTypes DefaultShapeType, AttributeDefinition[] Attributes, out String ErrorDescription)
        {
            ShapeFile sf = null;
            try
            {
                List<String> lExistantFiles, lNotExistantFiles;
                String BaseFileName = GetBaseFileName(FullFileName, out ErrorDescription, out lExistantFiles, out lNotExistantFiles);
                if (ErrorDescription != null)
                    return null;
                switch (lExistantFiles.Count)
                {
                    case 0:
                        break;
                    case 1:
                        ErrorDescription = "The file " + lExistantFiles[0] + " already exists.";
                        return null;
                    default:
                        ErrorDescription = "The following files already exist:";
                        foreach (String s in lExistantFiles)
                            ErrorDescription += Environment.NewLine + s;
                        return null;
                }

                switch (DefaultShapeType)
                {
                    case ShapefileTypes.Arc:
                    case ShapefileTypes.MultiPoint:
                    case ShapefileTypes.Point:
                    case ShapefileTypes.Polygon:
                        break;
                    default:
                        ErrorDescription = "The specified default shape type is not valid.";
                        return null;
                }

                List<AttributeDefinition> AList = new List<AttributeDefinition>();
                if (Attributes != null)
                {
                    ErrorDescription = "";
                    for (Int32 iAttribute = 0; iAttribute < Attributes.Length; iAttribute++)
                    {
                        String s = "at position " + ((iAttribute + 1).ToString());
                        AttributeDefinition A = Attributes[iAttribute];
                        if (A == null)
                        {
                            ErrorDescription += Environment.NewLine + "The attribute " + s + " is null.";
                            return null;
                        }
                        if (!String.IsNullOrEmpty(A.Name))
                            A.Name = A.Name.Trim().ToUpper();
                        if (String.IsNullOrEmpty(A.Name))
                        {
                            ErrorDescription += Environment.NewLine + "the attribute " + s + " has a null name.";
                        }
                        else
                        {
                            s += " (" + A.Name + ")";
                            if (A.Name.Length > 11)
                                ErrorDescription += Environment.NewLine + "the attribute " + s + " has a name longer than 11 characters (" + A.Name.Length.ToString() + ").";
                            foreach (AttributeDefinition a in AList)
                            {
                                if (a.Name.Equals(A.Name))
                                    ErrorDescription += Environment.NewLine + "the attribute " + s + " has been specified mode than once.";
                            }
                        }
                        switch (A.Type)
                        {
                            case AttributeTypes.Double:
                                if (A.Width < 1)
                                    ErrorDescription += Environment.NewLine + "the decimal attribute " + s + " must have a positive length.";
                                if (A.Decimals < 0)
                                    ErrorDescription += Environment.NewLine + "the decimal attribute " + s + " must have a non-negative number of decimals.";
                                else if (A.Width >= A.Decimals)
                                    ErrorDescription += Environment.NewLine + "the decimal attribute " + s + " must have a total length greater than the number of decimals.";
                                break;
                            case AttributeTypes.Integer:
                                if (A.Width < 1)
                                    ErrorDescription += Environment.NewLine + "the integer attribute " + s + " must have a positive length.";
                                A.Decimals = 0;
                                break;
                            case AttributeTypes.Logical:
                                if (A.Width < 1)
                                    ErrorDescription += Environment.NewLine + "the logical attribute " + s + " must have a positive length.";
                                A.Decimals = 0;
                                break;
                            case AttributeTypes.String:
                                if (A.Width < 1)
                                    ErrorDescription += Environment.NewLine + "the string attribute " + s + " must have a positive length.";
                                A.Decimals = 0;
                                break;
                            default:
                                ErrorDescription += Environment.NewLine + "the attribute " + s + " is of an invalid type.";
                                break;
                        }
                        AList.Add(A);
                    }
                    if (ErrorDescription.Length > 0)
                        return null;
                    ErrorDescription = null;
                }
                sf = new ShapeFile();
                sf.baseFileName = BaseFileName;
                sf.accessMode = AccessModes.ReadWrite;
                sf.shapefileType = DefaultShapeType;
                try
                {
                    sf.shxHandle = SHPCreate(sf.baseFileName, DefaultShapeType);
                    if (sf.shxHandle == 0)
                        throw new ApplicationException("Unable to create the shapefile .shp (or .shx) associated to " + sf.baseFileName);
                    sf.dbfHandle = DBFCreate(sf.baseFileName);
                    if (sf.dbfHandle == 0)
                        throw new ApplicationException("Unable to create the .dbf file associated to " + sf.baseFileName);
                    foreach (AttributeDefinition a in AList)
                        if (DBFAddField(sf.dbfHandle, a.Name, a.Type, a.Width, a.Decimals) < 0)
                            throw new ApplicationException("Error creating the field " + a.Name);
                    sf.attributes = AList;
                    return sf;
                }
                catch
                {
                    sf.Dispose();
                    sf = null;
                    try
                    { File.Delete(BaseFileName + ".shp"); }
                    catch
                    { }
                    try
                    { File.Delete(BaseFileName + ".shx"); }
                    catch
                    { }
                    try
                    { File.Delete(BaseFileName + ".dbf"); }
                    catch
                    { }
                    throw;
                }
            }
            catch (Exception x)
            {
                if (sf != null)
                {

                    sf.Dispose();
                }
                sf = null;
                ErrorDescription = ((x.InnerException == null) || String.IsNullOrEmpty(x.InnerException.Message)) ? x.Message : x.InnerException.Message;
                return null;
            }
        }

        /// <summary>Returns all the shapes contained in the shapefile.</summary>
        /// <returns>An array of shapes.</returns>
        public Shape[] GetAllShapes()
        {
            Int32 n = this.NumShapes;
            if (n == 0)
                return new Shape[] { };
            List<Shape> R = new List<Shape>(n);
            for (Int32 i = 0; i < n; i++)
                R.Add(new Shape(this, i));
            return R.ToArray();
        }

        /// <summary>Closes and frees any allocated resources.</summary>
        public void Dispose()
        {
            if (dbfHandle != 0)
            {
                try { DBFClose(dbfHandle); }
                catch { }
                dbfHandle = 0;
            }
            if (shxHandle != 0)
            {
                try { SHPClose(shxHandle); }
                catch { }
                shxHandle = 0;
            }
        }

        #endregion

        #region Enumeration
        private class ShapeList : IEnumerator
        {
            private Int32 index = -1;
            private ShapeFile parent;
            public ShapeList(ShapeFile Parent)
            {
                parent = Parent;
            }
            public void Reset()
            {
                index = -1;
            }
            public Boolean MoveNext()
            {
                return ++index < parent.NumShapes;
            }
            public Object Current
            {
                get
                {
                    return new Shape(parent, index);
                }
            }
            public void Dispose()
            {
                parent = null;
            }
        }
        /// <summary>Builds an enumerator to step into every shape in the shapefile.</summary>
        /// <returns>An enumerator for the shapes.</returns>
        public IEnumerator GetEnumerator()
        {
            return new ShapeList(this);
        }
        #endregion
    }

    /// <summary>The shape object contained in the shapefile.</summary>
    public class Shape
    {
        #region ShapeLib imports
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern IntPtr SHPReadObject(UInt32 ShpHandle, Int32 ShapeIndex);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern void SHPDestroyObject(IntPtr pObject);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern Int32 DBFIsAttributeNULL(UInt32 DbfHandle, Int32 ShapeIndex, Int32 FieldIndex);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern Double DBFReadDoubleAttribute(UInt32 DbfHandle, Int32 ShapeIndex, Int32 FieldIndex);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern Int32 DBFReadIntegerAttribute(UInt32 DbfHandle, Int32 ShapeIndex, Int32 FieldIndex);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern IntPtr DBFReadStringAttribute(UInt32 DbfHandle, Int32 ShapeIndex, Int32 FieldIndex);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern Int32 DBFWriteNULLAttribute(UInt32 DbfHandle, Int32 ShapeIndex, Int32 FieldIndex);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern Int32 DBFWriteIntegerAttribute(UInt32 DbfHandle, Int32 ShapeIndex, Int32 FieldIndex, Int32 FieldValue);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern Int32 DBFWriteDoubleAttribute(UInt32 DbfHandle, Int32 ShapeIndex, Int32 FieldIndex, Double FieldValue);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern Int32 DBFWriteStringAttribute(UInt32 DbfHandle, Int32 ShapeIndex, Int32 FieldIndex, IntPtr FieldValue);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern IntPtr SHPCreateSimpleObject(ShapeTypes Type, Int32 nVertices, IntPtr X, IntPtr Y, IntPtr Z);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern IntPtr SHPCreateObject(ShapeTypes Type, Int32 Index, Int32 nParts, IntPtr panPartStart, IntPtr panPartType, Int32 nVertices, IntPtr padfX, IntPtr padfY, IntPtr padfZ, IntPtr padfM);
        [DllImport("shapelib.dll", BestFitMapping = true, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = false)]
        private static extern Int32 SHPWriteObject(UInt32 ShpHandle, Int32 iShape, IntPtr pDllShape);
        #endregion

        #region Private variables
        private ShapeFile parent;
        private Int32 index;
        private ShapeTypes type;
        private PartList parts;
        private VertexList vertices;
        private Dictionary<AttributeDefinition, Object> _attributeValues = null;
        private Dictionary<AttributeDefinition, Object> attributeValues
        {
            get
            {
                if (_attributeValues == null)
                {
                    _attributeValues = new Dictionary<AttributeDefinition, Object>();
                    Int32 i = -1;
                    foreach (AttributeDefinition a in parent.Attributes)
                    {
                        i++;
                        if (DBFIsAttributeNULL(parent.DbfHandle, index, i) != 0)
                        {
                            _attributeValues[a] = null;
                        }
                        else
                        {
                            switch (a.Type)
                            {
                                case AttributeTypes.Double:
                                    _attributeValues[a] = DBFReadDoubleAttribute(parent.DbfHandle, index, i);
                                    break;
                                case AttributeTypes.Integer:
                                    _attributeValues[a] = DBFReadIntegerAttribute(parent.DbfHandle, index, i);
                                    break;
                                case AttributeTypes.Logical:
                                    throw new NotImplementedException();
                                case AttributeTypes.String:
                                    IntPtr p = DBFReadStringAttribute(parent.DbfHandle, index, i);
                                    String s = "";
                                    if (p != IntPtr.Zero)
                                        if ((s = Marshal.PtrToStringAnsi(p)) == null)
                                            s = "";
                                    _attributeValues[a] = s;
                                    break;
                                default:
                                    _attributeValues[a] = null;
                                    break;
                            }
                        }
                    }
                }
                return _attributeValues;
            }
        }
        #endregion

        #region Public classes

        /// <summary>A part of the shape.</summary>
        public class Part : VertexList
        {
            #region Private variables
            private PartTypes type;
            #endregion

            #region Public properties
            /// <summary>The type of this part of the shape.</summary>
            public PartTypes Type
            {
                get
                {
                    return type;
                }
            }
            #endregion

            #region Constructors
            /// <summary>Build a new part empty part of the shape.</summary>
            /// <param name="PartType">The type of the part to create.</param>
            public Part(PartTypes PartType)
                : base()
            {
                type = PartType;
            }
            /// <summary>Build a new part part of the shape pre-filled with some vertex.</summary>
            /// <param name="PartType">The type of the part to create.</param>
            /// <param name="PartVertices">A list of vertices to associate to this part of the shape.</param>
            public Part(PartTypes PartType, VertexList PartVertices)
                : base(PartVertices)
            {
                type = PartType;
            }
            #endregion
        }

        /// <summary>A list of shape parts.</summary>
        public class PartList : List<Part>
        {
            /// <summary>Adds a part to the end of the part list.</summary>
            /// <param name="item">The Part to be added at the end of the part list (null parts won't be added).</param>
            public new void Add(Part item)
            {
                if (item != null)
                    base.Add(item);
            }
            /// <summary>Adds the specified parts to the end of the part list.</summary>
            /// <param name="collection">The parts to be added at the end of the part list (null parts won't be added).</param>
            public new void AddRange(IEnumerable<Part> collection)
            {
                if (collection == null)
                    return;
                foreach (Part p in collection)
                    if (p != null)
                        base.Add(p);
            }
            /// <summary>Inserts the specified part at the specified position.</summary>
            /// <param name="index">Zero-based index where the part will be inserted.</param>
            /// <param name="item">The part to add (null parts won't be added).</param>
            public new void Insert(Int32 index, Part item)
            {
                if (item != null)
                    base.Insert(index, item);
            }
            /// <summary>Inserts the specified parts at the specified position.</summary>
            /// <param name="index">Zero-based index where the part will be inserted.</param>
            /// <param name="collection">The parts to be inserted into the part list (null parts won't be added).</param>
            public new void InsertRange(Int32 index, IEnumerable<Part> collection)
            {
                if (collection != null)
                {
                    foreach (Part p in collection)
                    {
                        if (p != null)
                            base.Insert(index++, p);
                    }
                }
            }
            /// <summary>The minimum area fully containing all the vertices of every part (null if no vertex/part is defined).</summary>
            public Area Limits
            {
                get
                {
                    Area aMax = null;
                    foreach (Part p in this)
                    {
                        if (aMax == null)
                            aMax = p.Limits;
                        else
                            aMax.ExpandTo(p.Limits);
                    }
                    return aMax;
                }
            }
        }

        #endregion

        #region Public properties
        /// <summary>The type of this shape.</summary>
        public ShapeTypes Type
        {
            get { return type; }
        }

        /// <summary>The list of the parts of this shape.</summary>
        public PartList Parts
        {
            get
            {
                return parts;
            }
        }
        /// <summary>The list of vertices of this shape.</summary>
        public VertexList Vertices
        {
            get
            {
                return vertices;
            }
        }

        #endregion

        #region Private types

        private struct dllShape : IDisposable
        {
            public Int32 nSHPType;
            public Int32 nShapeId;
            public Int32 nParts;
            public IntPtr panPartStart;
            public IntPtr panPartType;
            public Int32 nVertices;
            public IntPtr padfX;
            public IntPtr padfY;
            public IntPtr padfZ;
            public IntPtr padfM;
            public Double dfXMin;
            public Double dfYMin;
            public Double dfZMin;
            public Double dfMMin;
            public Double dfXMax;
            public Double dfYMax;
            public Double dfZMax;
            public Double dfMMax;
            public void Dispose()
            {
                nSHPType = 0;
                nShapeId = 0;
                nParts = 0;
                dfXMin = 0D;
                dfYMin = 0D;
                dfZMin = 0D;
                dfMMin = 0D;
                dfXMax = 0D;
                dfYMax = 0D;
                dfZMax = 0D;
                dfMMax = 0D;
                if (panPartStart != IntPtr.Zero)
                {
                    try
                    { Marshal.FreeHGlobal(panPartStart); }
                    catch
                    { }
                    panPartStart = IntPtr.Zero;
                }
                if (panPartType != IntPtr.Zero)
                {
                    try
                    { Marshal.FreeHGlobal(panPartType); }
                    catch
                    { }
                    panPartType = IntPtr.Zero;
                }
                nVertices = 0;
                if (padfX != IntPtr.Zero)
                {
                    try
                    { Marshal.FreeHGlobal(padfX); }
                    catch
                    { }
                    padfX = IntPtr.Zero;
                }
                if (padfY != IntPtr.Zero)
                {
                    try
                    { Marshal.FreeHGlobal(padfY); }
                    catch
                    { }
                    padfY = IntPtr.Zero;
                }
                if (padfZ != IntPtr.Zero)
                {
                    try
                    { Marshal.FreeHGlobal(padfZ); }
                    catch
                    { }
                    padfZ = IntPtr.Zero;
                }
                if (padfM != IntPtr.Zero)
                {
                    try
                    { Marshal.FreeHGlobal(padfM); }
                    catch
                    { }
                    padfM = IntPtr.Zero;
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>Return the value of a specified attribute.</summary>
        /// <param name="Attribute">The attribute for which to retrieve the value.</param>
        /// <returns>The value of the attribute.</returns>
        public Object GetAttribute(AttributeDefinition Attribute)
        {
            if (Attribute == null)
                throw new ArgumentNullException("attribute");
            return attributeValues[Attribute];
        }
        /// <summary>Return the value of a specified attribute.</summary>
        /// <param name="AttributeIndex">The 0-based attribute index for which to retrieve the value.</param>
        /// <returns>The value of the attribute.</returns>
        public Object GetAttribute(Int32 AttributeIndex)
        {
            return attributeValues[parent.Attributes[AttributeIndex]];
        }

        /// <summary>Saves the attributes of the shape to the Dbf file.</summary>
        /// <returns>true on success, false on error.</returns>
        public Boolean SaveAttributes()
        {
            String foo;
            return SaveAttributes(out foo);
        }
        /// <summary>Saves the attributes of the shape to the Dbf file.</summary>
        /// <param name="ErrorDescription">null on success, the error description if the function fails.</param>
        /// <returns>true on success, false on error.</returns>
        public Boolean SaveAttributes(out String ErrorDescription)
        {
            if (index < 0)
            {
                ErrorDescription = "The shape must be saved before saving its attributes.";
                return false;
            }
            if (parent.AccessMode != AccessModes.ReadWrite)
            {
                ErrorDescription = "The shapefile has been opened as read-only.";
                return false;
            }
            Int32 i = -1;
            foreach (KeyValuePair<AttributeDefinition, Object> kv in attributeValues)
            {
                i++;
                Boolean SavedOk;
                if (kv.Value == null)
                {
                    SavedOk = DBFWriteNULLAttribute(parent.DbfHandle, index, i) != 0;
                }
                else
                {
                    IntPtr pValue = IntPtr.Zero;
                    try
                    {
                        switch (kv.Key.Type)
                        {
                            case AttributeTypes.Double:
                                SavedOk = DBFWriteDoubleAttribute(parent.DbfHandle, index, i, Convert.ToDouble(kv.Value)) != 0;
                                break;
                            case AttributeTypes.Integer:
                                SavedOk = DBFWriteIntegerAttribute(parent.DbfHandle, index, i, Convert.ToInt32(kv.Value)) != 0;
                                break;
                            case AttributeTypes.String:
                                pValue = Marshal.StringToHGlobalAnsi(kv.Value as String);
                                SavedOk = DBFWriteStringAttribute(parent.DbfHandle, index, i, pValue) != 0;
                                Marshal.FreeHGlobal(pValue);
                                pValue = IntPtr.Zero;
                                break;
                            case AttributeTypes.Logical:
                                ErrorDescription = "Saving logical values is not supported.";
                                return false;
                            case AttributeTypes.Invalid:
                                ErrorDescription = "Saving invalid values is not supported.";
                                return false;
                            default:
                                ErrorDescription = "An attribute of unknown type has been found.";
                                return false;
                        }
                    }
                    catch (Exception x)
                    {
                        if (pValue != IntPtr.Zero)
                        {
                            try
                            { Marshal.FreeHGlobal(pValue); }
                            catch
                            { }
                            pValue = IntPtr.Zero;
                        }
                        ErrorDescription = ((x.InnerException == null) || String.IsNullOrEmpty(x.InnerException.Message)) ? x.Message : x.InnerException.Message;
                        return false;
                    }
                }
                if (!SavedOk)
                {
                    ErrorDescription = "Error saving the attribute " + i.ToString();
                    return false;
                }
            }
            ErrorDescription = null;
            return true;
        }
        /// <summary>Saves the shape (but not its attributes).</summary>
        /// <returns>true on success, false on error.</returns>
        public Boolean SaveShape()
        {
            String foo;
            return SaveShape(out foo);
        }
        /// <summary>Saves the shape (but not its attributes).</summary>
        /// <param name="ErrorDescription">null on success, the error description if the function fails.</param>
        /// <returns>true on success, false on error.</returns>
        public Boolean SaveShape(out String ErrorDescription)
        {
            if (parent.AccessMode != AccessModes.ReadWrite)
            {
                ErrorDescription = "The shapefile has been opened as read-only.";
                return false;
            }
            IntPtr pBytes = IntPtr.Zero;
            dllShape S = BuildToDllShape();
            try
            {
                Int32 nBytes = Marshal.SizeOf(S);
                pBytes = Marshal.AllocHGlobal(nBytes);
                Marshal.StructureToPtr(S, pBytes, false);
                Int32 newIndex = SHPWriteObject(parent.ShxHandle, index, pBytes);
                Marshal.FreeHGlobal(pBytes);
                pBytes = IntPtr.Zero;
                S.Dispose();
                if (newIndex < 0)
                {
                    ErrorDescription = "Error saving the shape.";
                    return false;
                }
                index = newIndex;
                ErrorDescription = null;
                return true;
            }
            catch (Exception x)
            {
                if (pBytes != IntPtr.Zero)
                {
                    try { Marshal.FreeHGlobal(pBytes); }
                    catch { }
                    pBytes = IntPtr.Zero;
                }
                S.Dispose();
                ErrorDescription = ((x.InnerException == null) || String.IsNullOrEmpty(x.InnerException.Message)) ? x.Message : x.InnerException.Message;
                return false;
            }
        }
        /// <summary>Saves the shape and its its attributes.</summary>
        /// <returns>true on success, false on error.</returns>
        public Boolean Save()
        {
            String foo;
            return Save(out foo);
        }
        /// <summary>Saves the shape and its attributes.</summary>
        /// <param name="ErrorDescription">null on success, the error description if the function fails.</param>
        /// <returns>true on success, false on error.</returns>
        public Boolean Save(out String ErrorDescription)
        {
            Boolean ok;
            if (ok = SaveShape(out ErrorDescription))
                ok = SaveAttributes(out ErrorDescription);
            return ok;
        }


        /// <summary>Create a new shape to be associated to the shapefile.</summary>
        /// <param name="Parent">The shapefile where this shape will be saved into.</param>
        /// <param name="ShapeType">The type of the shape.</param>
        public Shape(ShapeFile Parent, ShapeTypes ShapeType)
            : this(Parent, SHPCreateSimpleObject(ShapeType, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
        { }
        /// <summary>Loads a shape from the shapefile.</summary>
        /// <param name="Parent">The shapefile from where to load the shape.</param>
        /// <param name="ShapeIndex">The zero-based index of the shape.</param>
        public Shape(ShapeFile Parent, Int32 ShapeIndex)
            : this(Parent, SHPReadObject(Parent.ShxHandle, ShapeIndex))
        { }
        private Shape(ShapeFile Parent, IntPtr pShape)
        {
            if (pShape == IntPtr.Zero)
                throw new ApplicationException("Error creating a shape (null pointer).");
            try
            {
                parent = Parent;
                dllShape S = (dllShape)Marshal.PtrToStructure(pShape, typeof(dllShape));
                if (S.nSHPType < 0)
                    throw new ApplicationException("Error reading the shape data (bad type).");
                type = (ShapeTypes)S.nSHPType;
                index = S.nShapeId;
                if (S.nParts < 0)
                    throw new ApplicationException("Error reading the shape data (bad number of parts).");
                if (S.nVertices < 0)
                    throw new ApplicationException("Error reading the shape data (bad number of vertices).");
                this.parts = new PartList();
                this.vertices = new VertexList();
                if (S.nParts == 0)
                {
                    if (S.nVertices > 0)
                    {
                        Double[] lX = new Double[S.nVertices];
                        Marshal.Copy(S.padfX, lX, 0, S.nVertices);
                        Double[] lY = new Double[S.nVertices];
                        Marshal.Copy(S.padfY, lY, 0, S.nVertices);
                        Double[] lZ = new Double[S.nVertices];
                        Marshal.Copy(S.padfZ, lZ, 0, S.nVertices);
                        Double[] lM = new Double[S.nVertices];
                        Marshal.Copy(S.padfM, lM, 0, S.nVertices);
                        for (Int32 v = 0; v < S.nVertices; v++)
                            vertices.Add(new Vertex(lX[v], lY[v], lZ[v], lM[v]));
                    }
                }
                else
                {
                    Int32[] lStart = new Int32[S.nParts];
                    Marshal.Copy(S.panPartStart, lStart, 0, S.nParts);
                    Int32[] lType = new Int32[S.nParts];
                    Marshal.Copy(S.panPartType, lType, 0, S.nParts);
                    this.parts = new PartList();
                    if ((S.nParts == 1) && (S.nVertices == 0))
                    {
                        this.parts.Add(new Part((PartTypes)lType[0], new VertexList()));
                    }
                    else
                    {
                        if (S.nVertices < 0)
                            throw new ApplicationException("Errore durante la lettura dei dati.");
                        VertexList AllVertices = new VertexList();
                        if (S.nVertices > 0)
                        {
                            Double[] lX = new Double[S.nVertices];
                            Marshal.Copy(S.padfX, lX, 0, S.nVertices);
                            Double[] lY = new Double[S.nVertices];
                            Marshal.Copy(S.padfY, lY, 0, S.nVertices);
                            Double[] lZ = new Double[S.nVertices];
                            Marshal.Copy(S.padfZ, lZ, 0, S.nVertices);
                            Double[] lM = new Double[S.nVertices];
                            Marshal.Copy(S.padfM, lM, 0, S.nVertices);
                            for (Int32 v = 0; v < S.nVertices; v++)
                                AllVertices.Add(new Vertex(lX[v], lY[v], lZ[v], lM[v]));
                        }
                        for (Int32 p = 0; p < S.nParts; p++)
                        {
                            Int32 iStart = lStart[p];
                            Int32 iEnd = AllVertices.Count - 1;
                            for (Int32 q = 0; q < S.nParts; q++)
                                if ((q != p) && (lStart[q] > iStart))
                                    iEnd = Math.Min(iEnd, lStart[q] - 1);
                            if ((iStart < 0) || (iEnd >= AllVertices.Count))
                                throw new ApplicationException("Errore durante la lettura dei dati.");
                            this.parts.Add(new Part((PartTypes)lType[p], new VertexList(AllVertices.GetRange(iStart, iEnd - iStart + 1))));
                        }
                    }
                }

            }
            catch
            {
                if (pShape != IntPtr.Zero)
                {
                    try
                    { SHPDestroyObject(pShape); }
                    catch
                    { }
                    pShape = IntPtr.Zero;
                }
                throw;
            }
        }
        #endregion

        #region Private methods
        private dllShape BuildToDllShape()
        {
            dllShape d = new dllShape();
            try
            {
                d.nSHPType = (Int32)type;
                d.nShapeId = index;
                d.nVertices = 0;
                d.nParts = 0;
                List<Int32> Ps = new List<Int32>();
                List<Int32> Pt = new List<Int32>();
                List<Double> Vx = new List<Double>();
                List<Double> Vy = new List<Double>();
                List<Double> Vz = new List<Double>();
                List<Double> Vm = new List<Double>();
                if (parts.Count > 0)
                {
                    for (Int32 p = 0; p < parts.Count; p++)
                    {
                        if (parts[p].Count == 0)
                            continue;
                        d.nParts++;
                        Ps.Add(d.nVertices);
                        Pt.Add((Int32)parts[p].Type);
                        foreach (Vertex v in parts[p])
                        {
                            d.nVertices++;
                            Vx.Add(v.X);
                            Vy.Add(v.Y);
                            Vz.Add(v.Z);
                            Vm.Add(v.M);
                        }
                    }
                }
                else if (vertices.Count > 0)
                {
                    foreach (Vertex v in vertices)
                    {
                        d.nVertices++;
                        Vx.Add(v.X);
                        Vy.Add(v.Y);
                        Vz.Add(v.Z);
                        Vm.Add(v.M);
                    }
                }
                Area limits = Parts.Limits;
                d.dfXMin = limits.Left;
                d.dfXMax = limits.Right;
                d.dfYMin = limits.Top;
                d.dfYMax = limits.Bottom;
                d.dfZMin = limits.MinZ;
                d.dfZMax = limits.MaxZ;
                d.dfMMin = limits.MinM;
                d.dfMMax = limits.MaxM;
                if (d.nParts > 0)
                {
                    d.panPartStart = Marshal.AllocHGlobal(Ps.Count * Marshal.SizeOf(typeof(Int32)));
                    if (d.panPartStart == IntPtr.Zero) throw new OutOfMemoryException();
                    Marshal.Copy(Ps.ToArray(), 0, d.panPartStart, Ps.Count);

                    d.panPartType = Marshal.AllocHGlobal(Pt.Count * Marshal.SizeOf(typeof(Int32)));
                    if (d.panPartType == IntPtr.Zero) throw new OutOfMemoryException();
                    Marshal.Copy(Pt.ToArray(), 0, d.panPartType, Pt.Count);
                }
                if (d.nVertices > 0)
                {
                    d.padfX = Marshal.AllocHGlobal(Vx.Count * Marshal.SizeOf(typeof(Double)));
                    if (d.padfX == IntPtr.Zero) throw new OutOfMemoryException();
                    Marshal.Copy(Vx.ToArray(), 0, d.padfX, Vx.Count);

                    d.padfY = Marshal.AllocHGlobal(Vy.Count * Marshal.SizeOf(typeof(Double)));
                    if (d.padfY == IntPtr.Zero) throw new OutOfMemoryException();
                    Marshal.Copy(Vy.ToArray(), 0, d.padfY, Vy.Count);

                    d.padfZ = Marshal.AllocHGlobal(Vz.Count * Marshal.SizeOf(typeof(Double)));
                    if (d.padfZ == IntPtr.Zero) throw new OutOfMemoryException();
                    Marshal.Copy(Vz.ToArray(), 0, d.padfZ, Vz.Count);

                    d.padfM = Marshal.AllocHGlobal(Vm.Count * Marshal.SizeOf(typeof(Double)));
                    if (d.padfM == IntPtr.Zero) throw new OutOfMemoryException();
                    Marshal.Copy(Vm.ToArray(), 0, d.padfM, Vm.Count);
                }
                return d;
            }
            catch
            {
                d.Dispose();
                throw;
            }
        }
        #endregion
    }

    #endregion

}
