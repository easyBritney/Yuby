int main()
{
    int i ;
    int n = 0;
    for(i=0;i<5;i++)
    {
        if(i<2)
            continue;
        if(i>3)
            break;
        n=n+i;
    }
}