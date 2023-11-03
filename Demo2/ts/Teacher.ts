//Switching tabs
const teacherTabLinkList: NodeListOf<HTMLAnchorElement> = document.querySelectorAll('#sidebar-content .dashboard-item');
teacherTabLinkList.forEach(tabLink => {
    tabLink.addEventListener('click', (e) => {
        e.preventDefault();

        const tabList = document.querySelectorAll('.user-tab') as NodeListOf<HTMLElement>;
        tabList.forEach(tab => {
            tab.style.display = 'none';
        });


        // Remove is-active from all tab links
        teacherTabLinkList.forEach(link => {
            link.classList.remove('is-active');
        });

        // Add is-active to this current tab link
        tabLink.classList.add('is-active');

        //Get id target for each link
        const linkAnchor: HTMLAnchorElement | null = tabLink.querySelector('a');
        if (!linkAnchor) {
            return;
        }
        const target: string | null = linkAnchor.getAttribute('href');
        if (target === `/Home`) {
            window.location.href = `/Home`;
            return;
        }
        //Show the tab
        if (target) {
            const tab: HTMLElement | null = document.querySelector(target);
            if (tab) {
                tab.style.display = 'block';
            }
        }
    });
});

teacherTabLinkList[0].click();

//Teacher info form
const teacherAvatarElement = document.querySelector('.teacherAvatar') as HTMLInputElement;
const previewImageElement = document.getElementById('previewImage') as HTMLImageElement;
teacherAvatarElement.addEventListener('change', (event: Event) => {
    const files = (<HTMLInputElement>event.target).files;
    if (!files) {
        console.log('No file selected');
        return;
    }
    const file = files[0];
    let reader = new FileReader();
    // Set the onload function, which will be called after the file has been read
    reader.onload = (e) => {
        // The result attribute contains the data as a data: URL representing the file's data as a base64 encoded string.
        previewImageElement.src = <string>reader.result;
    };
    // Read the image file as a data URL
    reader.readAsDataURL(file);
});

class Account {
    accountId: string;
    username: string;
    password: string;
    role: string;
}

class Teacher {
    teacherId: string;
    accountId: string;
    avatar: string;
    fullName: string;
    information: string;
    contactNumber: string;
    email: string;
    account: Account;
}
var teacherId: string = `DAA3024B-DEC1-422F-A070-144253088AD8`;
const teacherInfoForm = document.getElementById('teacherInfoForm') as HTMLFormElement;
teacherInfoForm.addEventListener('submit', async (e: Event) => {
    e.preventDefault();
    const passwordElement = document.getElementById('password') as HTMLInputElement;
    const repassElement = document.getElementById('repass') as HTMLInputElement;
    if (passwordElement.value !== repassElement.value) {
        alert('Vui lòng xác nhận mật khẩu chính xác');
        return;
    }
    await saveTeacherInfo();
});
async function saveTeacherInfo() {
    const url = `https://localhost:7235/api/teacher/update/${teacherId}`;
    const formData = new FormData(teacherInfoForm);
    const response = await fetch(url, {
        method: 'PUT',
        body: formData
    });
    if (response.status !== 204) {
        throw new Error(`Http Error! Status code: ${response.status}`);
    }
    alert('Lưu thông tin thành công');
}

async function fetchTeacherInfoData() {
    const url = `https://localhost:7235/api/teacher/${teacherId}`;
    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        if (!response.ok) {
            throw new Error(`Error! Status code: ${response.status}`);
        }
        const data = await response.json();
        console.log(data);
        const teacher: Teacher = data;
        loadTeacherData(teacher);
    } catch (error) {
        console.error(error);
    }
}

function loadTeacherData(teacher: Teacher) {
    const fullNameElement = document.getElementById('fullname') as HTMLInputElement;
    const previewImageElement = document.getElementById('previewImage') as HTMLImageElement;
    const phoneNumberElement = document.getElementById('phone') as HTMLInputElement;
    const emailElement = document.getElementById('email') as HTMLInputElement;
    const descriptionElement = document.getElementById('description') as HTMLTextAreaElement;
    const passwordElement = document.getElementById('password') as HTMLInputElement;

    fullNameElement.value = teacher.fullName;
    previewImageElement.src = `/img/avatar/${teacher.avatar}`;
    phoneNumberElement.value = teacher.contactNumber;
    emailElement.value = teacher.email;
    descriptionElement.textContent = teacher.information;
    passwordElement.value = teacher.account.password;
}

function getCalendarDays(month: number, year: number): (Date | null)[] {
    const date = new Date(year, month - 1, 1);
    const days: (Date | null)[] = [];

    // Find the first Sunday before the month starts
    while (date.getDay() !== 0) {
        days.push(null);
        date.setDate(date.getDate() - 1);
    }

    const currentMonth = date.getMonth();

    // Reset date to the first day of the month
    date.setDate(1);

    while (date.getMonth() === currentMonth) {
        days.push(new Date(date));
        date.setDate(date.getDate() + 1);
    }

    // Fill the array to complete the last week with the next month's days
    while (days.length % 7 !== 0) {
        days.push(null);
    }

    return days;
}


function displayCalendar(month: number, year: number): void {
    let timeTableBody = document.getElementById('teacherTimetable') as HTMLTableSectionElement;
    timeTableBody.innerHTML = '';
    let days = getCalendarDays(month, year);
    let tr: HTMLTableRowElement | null = null;

    for (let i = 0; i < days.length; i++) {
        if (i % 7 === 0) { // start a new row every week
            tr = document.createElement('tr');
            tr.className = 'text-center h-20';
        }

        if (tr !== null) {
            if (days[i] !== null) {
                let normalCellTemplate = document.getElementById('normalDayCellTemplate') as HTMLTemplateElement;
                let normalCellClone = document.importNode(normalCellTemplate.content, true);
                let dayText = normalCellClone.querySelector('.dayText') as HTMLSpanElement;
                if (dayText) {
                    dayText.textContent = days[i]?.getDate().toString() || '';
                }
                tr.appendChild(normalCellClone);
            } else {
                let missingCellTemplate = document.getElementById('missingDayCellTemplate') as HTMLTemplateElement;
                let missingCellClone = document.importNode(missingCellTemplate.content, true);
                if (timeTableBody) {
                    tr.appendChild(missingCellClone);
                }
            }

            if (i % 7 === 6 || i === days.length - 1) {
                if (timeTableBody && tr !== null) {
                    timeTableBody.appendChild(tr);
                }
            }
        }
    }
}
const userId: string = `1AD7482E-2EB6-4394-B63A-D22120241532`;
class Schedule {
    scheduleId: string;
    hireId: string;
    licenseId: string;
    startTime: string;
    endTime: string;
    date: Date;
    address: string;
    status: string;
}
const year: number = 2023;
async function fetchScheduleData(month: number) {
    const url: string = `https://localhost:7235/api/user/schedules/${userId}?month=${month}`;
    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        if (!response.ok) {
            throw new Error(`HTTP Error! Status code: ${response.status}`);
        }
        const data = await response.json();
        console.log(data);
        var scheduleList: Schedule[] = data;
        displayCalendar(month, year);
        renderScheduleData(scheduleList, month);
        const normalDayCells = document.querySelectorAll('td') as NodeListOf<HTMLTableCellElement>;
        normalDayCells.forEach(normalDayCell => {
            normalDayCell.addEventListener('click', async () => {
                console.log('Click event triggered');
                const dayTextElement = normalDayCell.querySelector('span') as HTMLSpanElement;
                if (dayTextElement.textContent.trim() !== ``) {
                    const day = Number(dayTextElement.textContent);
                    const month: number = Number(monthSelect.value);
                    console.log(`${day}-${month}`);
                    await fetchScheduleDataForDay(day, month);
                }
                toggleScheduleDetailsModal();
            });
        });
    } catch (error) {
        console.error(error);
    }
}

async function fetchScheduleDataForDay(day: number, month: number) {
    const dateParam = `${year}-${month}-${day}`;
    const url: string = `https://localhost:7235/api/user/schedules/date/${userId}?date=${dateParam}`;
    console.log('Date to fetch: ' + dateParam);
    console.log(url);
    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        if (!response.ok) {
            throw new Error(`HTTP Error! Status code: ${response.status}`);
        }
        const data = await response.json();
        console.log(data);
        var scheduleList: Schedule[] = data;
        renderSchedulesForDay(scheduleList);
    } catch (error) {
        console.error(error);
    }
}

function toggleScheduleDetailsModal() {
    const modal = document.getElementById('detailsScheduleModal') as HTMLDivElement;
    const opened = (!modal.classList.contains('hidden') && modal.classList.contains('flex') && modal.classList.contains('blur-background'));
    if (!opened) {
        modal.classList.remove('hidden');
        modal.classList.add('flex');
        modal.style.justifyContent = `center`;
        modal.style.alignItems = `center`;
        modal.classList.add('blur-background');
    } else {
        modal.classList.add('hidden');
        modal.classList.remove('flex');
        modal.classList.remove('blur-background');
    }
}

function getFormattedTime(timeString: string) {
    let formattedTime = timeString.substring(0, 5);
    return formattedTime;
}

function renderSchedulesForDay(scheduleList: Schedule[]) {
    const scheduleDetailsModalContent = document.getElementById('scheduleDetailsModalContent') as HTMLOListElement;
    scheduleDetailsModalContent.innerHTML = ``;
    scheduleList.forEach(schedule => {
        const scheduleDetailsTemplate = document.getElementById('scheduleDetailsTemplate') as HTMLTemplateElement;
        let scheduleDetailsElementClone = document.importNode(scheduleDetailsTemplate.content, true);
        let courseNameElement = scheduleDetailsElementClone.querySelector('.courseName') as HTMLSpanElement;
        let courseStatusElement = scheduleDetailsElementClone.querySelector('.courseStatus') as HTMLSpanElement;
        let courseDateElement = scheduleDetailsElementClone.querySelector('.courseDate') as HTMLSpanElement;
        let courseTimeElement = scheduleDetailsElementClone.querySelector('.courseTime') as HTMLSpanElement;

        courseNameElement.textContent = `Khóa ${schedule.licenseId}`;

        const doneStatusClassName = `bg-green-100 text-green-800 text-sm font-medium mr-2 px-2.5 py-0.5 rounded dark:bg-green-900 dark:text-green-300 ml-3`;
        const notDoneStatusClassName = `bg-red-100 text-red-800 text-sm font-medium mr-2 px-2.5 py-0.5 rounded dark:bg-red-900 dark:text-red-300 ml-3`;
        if (schedule.status === `Sắp tới`) {
            courseStatusElement.className = `courseStatus ${doneStatusClassName}`;
        } else {
            courseStatusElement.className = `courseStatus ${notDoneStatusClassName}`;
        }
        courseStatusElement.textContent = schedule.status;

        courseDateElement.textContent = ``;

        courseTimeElement.textContent = `${getFormattedTime(schedule.startTime)}~${getFormattedTime(schedule.endTime)}`;
        scheduleDetailsModalContent.appendChild(scheduleDetailsElementClone);

    });
}

function renderScheduleData(scheduleList: Schedule[], month: number) {
    if (scheduleList === null || scheduleList.length === 0) {
        console.log('No schedules data');
        return;
    }
    const normalDayElements = document.querySelectorAll('.normalDay') as NodeListOf<HTMLTableCellElement>;
    scheduleList.forEach(schedule => {
        const scheduleDate: Date = new Date(schedule.date);
        const index = scheduleDate.getDate() - 1;
        const normalDayCell = normalDayElements[index];
        const eventsContainer = normalDayCell.querySelector('.eventsContainer') as HTMLDivElement;
        let eventTemplate = document.getElementById('event-template') as HTMLTemplateElement;
        let eventElementClone = document.importNode(eventTemplate.content, true);

        var eventNameElement = eventElementClone.querySelector('.event-name') as HTMLSpanElement;
        eventNameElement.textContent = `Khóa ${schedule.licenseId}`;

        var timeElement = eventElementClone.querySelector('time') as HTMLTimeElement;
        // timeElement.textContent = `${schedule.startTime}~${schedule.endTime}`;
        timeElement.textContent = `${getFormattedTime(schedule.startTime)}~${getFormattedTime(schedule.endTime)}`;

        eventsContainer.appendChild(eventElementClone);
    });
}



const monthSelect = document.getElementById('monthSelect') as HTMLSelectElement;
monthSelect.addEventListener('input', () => {
    if (monthSelect.value === "") {
        alert('Vui lòng chọn tháng phù hợp');
        return;
    }
    const month: number = Number(monthSelect.value);
    fetchScheduleData(month);
});


document.addEventListener('DOMContentLoaded', async () => {
    var currentDate = new Date();
    var currentMonth = currentDate.getMonth() + 1;
    monthSelect.selectedIndex = currentMonth;
    await fetchScheduleData(currentMonth);

});