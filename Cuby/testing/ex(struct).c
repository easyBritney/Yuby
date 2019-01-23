struct student{
    int number;
    int number2;
    char name[5];
    float id;
};
int main(){
    struct student hello;
    hello.number = 10;
    hello.id = 234;
    hello.name[4] = 'c';
    hello.name[0] = 'a';
    print hello.number;
    print hello.name[4];
}