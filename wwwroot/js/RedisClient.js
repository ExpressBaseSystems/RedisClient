var RedisClient = function (option) {
    this.X = "10000";

    this.init = function () {
        $("#btnkeys").off("click").on("click", this.btnKeyClick.bind(this));
    };
    
    this.btnKeyClick = function () {
        $.ajax({
            url: "EbRedisManager/Kllkeys",
            data: null,
            cache: false,
            type: "POST",
            success: this.btnKeyAjaxS.bind(this)
        });

        this.btnKeyAjaxS = function (dict) {
            $('#tableDiv').empty()
            let html = `<table border="1" width="100" style="width:100% ">
                                                            <tr> <td>KEY</td><td>VALUE</td></tr>`;
            $.each(dict, function (i, k) {
                html += `<tr> <td>${i}</td><td>${k}</td></tr>`;
            });
            html += `</table>`;
            $("#tableDiv").append(html);
        }
    };

    this.init();
};