import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({ selector: 'app-pagination', standalone: true, template: `<div class="pagination"><button class="secondary" [disabled]="page <= 1" (click)="pageChange.emit(page - 1)">Previous</button><span>Page {{ page }} of {{ totalPages }}</span><button class="secondary" [disabled]="page >= totalPages" (click)="pageChange.emit(page + 1)">Next</button></div>`, styles: [` .pagination{display:flex;align-items:center;justify-content:center;gap:10px;flex-wrap:wrap}.pagination span{min-width:85px;text-align:center;color:#555;font-size:.9rem}.pagination button{font-size:.88rem;padding:8px 11px}@media(max-width:420px){.pagination{justify-content:flex-start}}`] })
export class PaginationComponent { @Input() page = 1; @Input() totalPages = 1; @Output() pageChange = new EventEmitter<number>(); }

