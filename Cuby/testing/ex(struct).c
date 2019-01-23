struct student{
    int number;
    char name[5];
    float id;
};
int main(){
    int i;

    struct student hello;
    hello.number = 10;
    hello.id = 234;
    hello.name[1] = 'c';
    int j;
    print hello.number;
    print hello.name[1];
}