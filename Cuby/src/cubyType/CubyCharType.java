package cubyType;

public class CubyCharType extends CubyBaseType {
    private char value;

    CubyCharType(){

    }

    public CubyCharType(char c){
        value = c;
    }

    public char getValue() {
        return value;
    }

    public void setValue(char value) {
        this.value = value;
    }
}
