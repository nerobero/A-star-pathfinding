using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

public class PathNode
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathNode parent;

    public PathNode(MapLocation loc, float g, float h, float f, GameObject mark, PathNode par)
    {
        location = loc;
        G = g;
        H = h;
        F = f;
        marker = mark;
        parent = par;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return location.Equals(((PathNode)obj).location);
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }
}

public class AStarPathfinding : MonoBehaviour
{
    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;

    private List<PathNode> open = new List<PathNode>();
    private List<PathNode> closed = new List<PathNode>();

    public GameObject sourceNode;
    public GameObject destNode;
    public GameObject pathNode;

    private PathNode src;
    private PathNode dest;
    private PathNode lastPosition;

    private bool done = false;

    void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers)
        {
            Destroy(m);
        }
    }

    void BeginSearch()
    {
        done = false;
        RemoveAllMarkers();

        List<MapLocation> loMapLoc = new List<MapLocation>();

        for (int z = 1; z < maze.depth - 1; z++)
            for (int x = 1; x < maze.width - 1; x++)
            {
                if (maze.map[x, z] != 1)
                {
                    loMapLoc.Add(new MapLocation(x,z));
                }
            }

        loMapLoc.Shuffle();

        Vector3 srclocation = new Vector3(loMapLoc[0].x* maze.scale, 0, loMapLoc[0].z* maze.scale);
        src = new PathNode(new MapLocation(loMapLoc[0].x, loMapLoc[0].z), 0, 0, 0,
                               Instantiate(sourceNode,srclocation,Quaternion.identity), null);
        
        Vector3 destlocation = new Vector3(loMapLoc[1].x* maze.scale, 0, loMapLoc[1].z* maze.scale);
        dest = new PathNode(new MapLocation(loMapLoc[1].x, loMapLoc[1].z), 0, 0, 0,
            Instantiate(destNode,destlocation,Quaternion.identity), null);

        open.Clear(); 
        closed.Clear();
        
        open.Add(src); //made a mistake in the slides, it is supposed to be added to the open list!
        lastPosition = src;
    }

    void Search(PathNode thisNode)
    {
        if (thisNode.Equals(dest) || open.Count == 0)
        {
            done = true;
            return; //we have found the dest and thus the search is done!
        }

        foreach (MapLocation dir in maze.directions) //allows WASD directions
        {
            MapLocation neighbour = dir + thisNode.location;
            
            //if the neighbour node is not accesible, we ignore!
            if (maze.map[neighbour.x, neighbour.z] == 1) continue; //if wall, then ignore
            if (neighbour.x < 1 || neighbour.x >= maze.width || neighbour.z < 1 || neighbour.z >= maze.depth) continue; //if out of bounds ignore. 
            if (IsClosed(neighbour)) continue;

            float G = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
            float H = Vector2.Distance(neighbour.ToVector(), dest.location.ToVector());
            float F = G + H;

            GameObject pathBlock = Instantiate(pathNode, new Vector3(neighbour.x * maze.scale,
                0, neighbour.z * maze.scale), Quaternion.identity);

            if (!UpdateMarker(neighbour, G, H, F, thisNode))
            {
                open.Add(new PathNode(neighbour,G,H,F,pathBlock,thisNode));
            }
        }

        open = open.OrderBy(p => p.F).ToList<PathNode>();
        PathNode pn = (PathNode)open.ElementAt(0);
        closed.Add(pn);
        open.RemoveAt(0);
        pn.marker.GetComponent<Renderer>().material = closedMaterial;
        lastPosition = pn;
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathNode prt)
    {
        foreach (PathNode p in open)
        {
            if (p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }

        return false;
    }

    bool IsClosed(MapLocation marker)
    {
        foreach (PathNode p in closed)
        {
            if (p.location.Equals(marker)) return true;
        }

        return false;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) BeginSearch();
        if (Input.GetKeyDown(KeyCode.S) && !done) Search(lastPosition);
        if (Input.GetKeyDown(KeyCode.P)) GetPath();
    }

    private void GetPath()
    {
        RemoveAllMarkers();
        PathNode begin = lastPosition;

        while (!src.Equals(begin) && begin != null)
        {
            Instantiate(pathNode, new Vector3(src.location.x * maze.scale, 0, src.location.z * maze.scale),
                Quaternion.identity);
            begin = begin.parent;
        }
        Instantiate(pathNode, new Vector3(src.location.x * maze.scale, 0, src.location.z * maze.scale),
            Quaternion.identity);
    }
}
