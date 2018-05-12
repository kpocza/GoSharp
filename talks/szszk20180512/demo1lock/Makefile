OBJ=lock.o
OUT=lock

all: build

$(OBJ): %.o: %.c
	gcc $< -c -o $@

build: $(OBJ)
	gcc -o $(OUT) $^ -lpthread

clean:
	rm -f $(OBJ)
	rm -f $(OUT)


