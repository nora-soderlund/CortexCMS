(async () => {
    const user = await API.fetchAsync("user/authorize");

    console.log(user);
})();
