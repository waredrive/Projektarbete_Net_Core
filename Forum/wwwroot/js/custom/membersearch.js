$(document).ready(() => {
  var searchResults = new Bloodhound({
    datumTokenizer: Bloodhound.tokenizers.obj.whitespace("username"),
    queryTokenizer: Bloodhound.tokenizers.whitespace,
    remote: {
      url: "/profile/search?query=%QUERY",
      wildcard: "%QUERY"
    }
  });

    $("#memberSearch .typeahead").typeahead({
      hint: true,
      highlight: true
  },
    {
      name: "members",
      display: "username",
      source: searchResults,
      templates: {
        empty: function (context) {
          $(".tt-dataset").text("No such member");
        }
      }
    });

  $("#memberSearch .typeahead").bind("typeahead:select", (ev, suggestion) => {
    var returnUrl = window.location.href;

    if (returnUrl.indexOf("?returnUrl=") !== -1) {
      returnUrl = returnUrl.split("?returnUrl=").pop();
    }

    window.location.href = `/profile/details/${suggestion["username"]}/?returnUrl=${returnUrl}`;
  });

  $("#memberSearch .typeahead").on("keypress", function (evt) {
    if (evt.which === 13) {
      var typeahead = $(this).data().ttTypeahead;
      var menu = typeahead.menu;
      var sel = menu.getActiveSelectable() || menu.getTopSelectable();
      if (menu.isOpen()) {
        menu.trigger("selectableClicked", sel);
        evt.preventDefault();
      }
    }
  });
});