import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { MatTableDataSource, MatSort } from '@angular/material';
import { SelectionModel } from '@angular/cdk/collections';

@Component({
  selector: 'app-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.css']
})

export class TableComponent implements OnInit {

  @Input() displayedColumns: string[];
  @Input() dataSource: any[];
  @Output() update = new EventEmitter();
  @Output() delete = new EventEmitter();
  @Output() selectEvent = new EventEmitter();
  @Input() hasUpdate: boolean;
  @Input() hasDelete: boolean;
  tableDataSource: MatTableDataSource<any>;
  totalDisplayedColumns: string[] = [];
  initialSelection = [];
  allowMultiSelect = true;
  selection = new SelectionModel<any>(this.allowMultiSelect, this.initialSelection);

  @ViewChild(MatSort, { static: true }) sort: MatSort;

  constructor() {
  }

  ngOnInit(): void {
    if (this.dataSource.length > 0) {
      this.tableDataSource = new MatTableDataSource(this.dataSource);
      this.tableDataSource.sort = this.sort;
      this.totalDisplayedColumns = ['Select', ...this.displayedColumns];
      if (this.hasUpdate || this.hasDelete) {
        this.totalDisplayedColumns = [...this.totalDisplayedColumns, 'Actions'];
      }
    }
  }

  getValueAtIndex(entity: any, index: number) {
    return entity[Object.keys(entity)[index]];
  }

  updateElement = (row) => {
    this.update.emit(row);
  }

  deleteElement = (row) => {
    this.delete.emit(row);
  }

  selectElement = (row) => {
    this.selection.toggle(row);
    this.selectEvent.emit(this.selection.selected);
  }

  applyFilter = (filterValue: string) => {
    this.tableDataSource.filter = filterValue.trim().toLowerCase();
  }

  isAllSelected = () => {
    const numSelected = this.selection.selected.length;
    const numRows = this.tableDataSource.data.length;
    return numSelected === numRows;
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  masterToggle = () => {
    this.isAllSelected() ?
      this.selection.clear() :
      this.tableDataSource.data.forEach(row => this.selection.select(row));
    this.selectEvent.emit(this.selection.selected);
  }

}
