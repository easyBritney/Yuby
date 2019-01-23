int fact(int i){
    if(i == 1){
        return 1;
    }else{
        return i * fact(i - 1);
    }
}

int main(){
    int n = 100;
    fact(n);
}