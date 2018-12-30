﻿using System;

namespace Forum.Models.Pagination {
  public class Pager {
    public int TotalItems { get; private set; }
    public int CurrentPage { get; private set; }
    public int PageSize { get; private set; }
    public int TotalPages { get; private set; }
    public int StartPage { get; private set; }
    public int EndPage { get; private set; }

    public Pager(int totalItems, int? page, int pageSize = 15) {
      var totalPages = (int)Math.Ceiling(decimal.Divide(totalItems, pageSize));
      var currentPage = page ?? 1;
      var startPage = currentPage - 5;
      var endPage = currentPage + 4;
      if (startPage <= 0) {
        endPage -= (startPage - 1);
        startPage = 1;
      }

      if (endPage > totalPages) {
        endPage = totalPages;
        if (endPage > 10) {
          startPage = endPage - 9;
        }
      }

      TotalItems = totalItems;
      CurrentPage = currentPage;
      PageSize = pageSize;
      TotalPages = totalPages;
      StartPage = startPage;
      EndPage = endPage;
    }
  }
}
