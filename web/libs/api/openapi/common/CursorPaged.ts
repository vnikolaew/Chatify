export interface CursorPaged<T> {
   pagingCursor?: string;
   pageSize: number;
   items: T[];
   hasMore: boolean;
   total: number;
}
