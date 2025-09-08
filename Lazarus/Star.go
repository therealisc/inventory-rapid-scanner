// This can be one of the biggest mistakes of my life... because I treated others instead of treating myself


package main

import (
	"fmt"
	//"io"
	//"os"

	//"github.com/Valentin-Kaiser/go-dbase/dbase"
)

type Article struct {
	// The dbase tag contains the table name and column name separated by a dot.
	// The column name is case insensitive.
	//ID          int32     `dbase:"TEST.PRODUCTID"`
	Cod         string    `dbase:"ARTICOLE.COD"`
	Name        string    `dbase:"ARTICOLE.DENUMIRE"`
	Price       float64   `dbase:"ARTICOLE.PRET_VANZ"`
	Barcode     int64     `dbase:"ARTICOLE.COD_BARE"`
	Stoc        int64     `dbase:"ARTICOLE.STOC"`
	//Double      float64   `dbase:"TEST.DOUBLE"`
	//Date        time.Time `dbase:"TEST.DATE"`
	//DateTime    time.Time `dbase:"TEST.DATETIME"`
	//Integer     int32     `dbase:"TEST.INTEGER"`
	//Float       float64   `dbase:"TEST.FLOAT"`
	//Active      bool      `dbase:"TEST.ACTIVE"`
	//Description string    `dbase:"TEST.DESC"`
	//Tax         float64   `dbase:"TEST.TAX"`
	//Stock       int64     `dbase:"TEST.INSTOCK"`
	//Blob        []byte    `dbase:"TEST.BLOB"`
	//Varbinary   []byte    `dbase:"TEST.VARBIN_NIL"`
	//Varchar     string    `dbase:"TEST.VAR_NIL"`
	//Var         string    `dbase:"TEST.VAR"`
}

func main() {
	// Open debug log file so we see what's going on
	//f, err := os.OpenFile("debug.log", os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0644)
	//if err != nil {
	//	fmt.Println(err)
	//	return
	//}

	//dbase.Debug(true, io.MultiWriter(os.Stdout, f))
	table, err := dbase.OpenTable(&dbase.Config{
		//Filename:   "C:\\SAGA C.3.0\\0003\\ARTICOLE.DBF",
		Filename:   "/home/therealisc/0004/INTRARI.DBF",
		TrimSpaces: true,
	})
	if err != nil {
		panic(err)
	}
	defer table.Close()

	fmt.Printf(
		"Last modified: %v Columns count: %v Record count: %v File size: %v \n",
		table.Header().Modified(0),
		table.Header().ColumnsCount(),
		table.Header().RecordsCount(),
		table.Header().FileSize(),
	)

	// Print all database column infos.
	for _, column := range table.Columns() {
		fmt.Printf("Name: %v - Type: %v \n", column.Name(), column.Type())
	}

	return;

	// Loop through all rows using rowPointer in DBF struct.
	for !table.EOF() {
		row, err := table.Next()
		if err != nil {
			panic(err)
		}

		// Skip deleted rows.
		if row.Deleted {
			fmt.Printf("Deleted row at position: %v \n", row.Position)
			continue
		}

		if p.Cod == "00001381" {
			fmt.Printf("Product: %+v \n", p)
		}
	}

}
