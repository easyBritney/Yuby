int main()
{
    try{
        int a=1;
        int n=33;
        n=a/0;
    }
    catch("ArithmeticalExcption")
    {
        n=0;
        //print n;
    }   
}