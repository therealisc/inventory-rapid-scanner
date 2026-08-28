#include <stdio.h>
//#include <sql.h>


int main() {
    int dec = 0;
	int bit = 1;
	int n = 10000000;

	while (n) {
		int last_digit = n % 10;
		n = n / 10;
		dec += last_digit * bit;
		bit = bit * 2;
	}
	    
	printf("%d\n", dec);
	printf("%d\n", dec);

    return dec;
}
