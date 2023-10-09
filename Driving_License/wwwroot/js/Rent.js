const c = document.querySelector('.container')
const indexs = Array.from(document.querySelectorAll('.index'))
let cur = -1
indexs.forEach((index, i) => {
    index.addEventListener('click', (e) => {
        // clear
        c.className = 'container'
        void c.offsetWidth; // Reflow
        c.classList.add('open')
        c.classList.add(`i${i + 1}`)
        if (cur > i) {
            c.classList.add('flip')
        }
        cur = i
    })
})

var party1Input = document.getElementById("party1");
var party2Input = document.getElementById("party2");

// Tạo một đối tượng Date hiện tại
var currentDate = new Date();

// Định dạng ngày và giờ hiện tại thành chuỗi phù hợp cho datetime-local
var currentDateString = currentDate.toISOString().slice(0, 16);

// Đặt giá trị mặc định cho thẻ input datetime-local
if (party1Input !== null) {
    party1Input.value = currentDateString;
}
if (party2Input !== null) {
    party2Input.value = currentDateString;
}


const rentForm = document.querySelector('.rentForm');
const submitBtn = document.querySelector('.submit-btn');
submitBtn.addEventListener('click', function(e) {
    const startDate = document.querySelector('#party1').value;
    const endDate = document.querySelector('#party2').value;
    const hasValidFormData = (startDate !== null && endDate !== null);
    if (!hasValidFormData) {
        alert('Vui lòng điền thông tin thuê đầy đủ');
        e.preventDefault();
        return;
    }
    const confirmSubmit = window.confirm('Xác nhận thuê?');
    if (confirmSubmit) {
        rentForm.submit();
        return;
    } else {
        e.preventDefault();
    }
});