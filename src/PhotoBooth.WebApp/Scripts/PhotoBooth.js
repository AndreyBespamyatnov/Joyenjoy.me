$(function () { // will trigger when the document is ready
    $('.datepicker').datetimepicker({
        locale: 'ru',
        sideBySide: true
    }); //Initialise any date pickers

    $.validator.methods.date = function (value, element) {
        return this.optional(element) || Globalize.parseDate(value);
    }
});
