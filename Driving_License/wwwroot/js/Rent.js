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
party1Input.value = currentDateString;
party2Input.value = currentDateString;

