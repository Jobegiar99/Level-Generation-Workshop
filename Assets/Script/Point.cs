using System;

public class Point : IEquatable<Point>, IComparable<Point>
{

    public int row;
    public int column;

    public Point(int row, int column)
    {
        this.row = row;
        this.column = column;
    }


    public bool Equals(Point other)
    {
        //
        // See the full list of guidelines at
        //   http://go.microsoft.com/fwlink/?LinkID=85237
        // and also the guidance for operator== at
        //   http://go.microsoft.com/fwlink/?LinkId=85238
        //

        if (other == null || GetType() != other.GetType())
        {
            return false;
        }

        // TODO: write your implementation of Equals() here
        return (this.row == other.row && this.column == other.column);
    }

    // override object.GetHashCode
    public int GetHashCode()
    {
        // TODO: write your implementation of GetHashCode() here
        return base.GetHashCode();
    }
    
    public int CompareTo(Point other)
    {
        return (other.row == this.row) ? 1 : 0;
    }

}
    

