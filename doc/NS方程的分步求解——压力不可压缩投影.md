

# NS方程的分步求解——压力/不可压缩投影

先来看看我们要求解的压力/不可压缩投影方程：
$$
\frac{\partial \vec{u}}{\partial t}+\frac{1}{\rho}\nabla p = 0\\ 

s.t. \nabla \cdot \vec{u}=0
$$
我们希望最终算出的速度$\vec{u}^{n+1}$ 是无散度的，具体步骤如下：

- 用压力梯度$\nabla p$更新下一个时间步长的速度$\vec{u}^{n+1}$
- 将$\vec{u}^{n+1}$代入无散度公式$\nabla \cdot \vec{u}=0$
- 求出满足速度场无散度的压力场$p$
- 用上一步求出的压力场p再一次更新速度$\vec{u}^{n+1}$

下面我们来看具体的离散化求解步骤：

首先，这里声明一下，为了看起来简单一点，我们统一把表示当前时间步长的上标$n$省略，如果是表示下一个时间步长的值的，我们会用上标$n+1$明确标出来。

接下来，我们来看采用前向欧拉法对**压力梯度**进行离散化的操作，得到：
$$
\nabla p = (\frac {p_{i+1,j}-p_{i,j}}{\Delta x},\frac {p_{i,j+1}-p_{i,j}}{\Delta x})
$$
然后，根据散度的定义对速度散度进行离散化，得到：
$$
\nabla \cdot \vec{u}=\frac{\partial u}{\partial x}+\frac{\partial v}{\partial y} \approx \frac{u_{i+1/2,j}-u_{i-1/2,j}}{\Delta x}+\frac{v_{i+1/2,j}-v_{i-1/2,j}}{\Delta x} =0
$$
用第$n+1$个时间步长的速度来表示，得到：
$$
\frac{u_{i+1/2,j}^{n+1}-u_{i-1/2,j}^{n+1}}{\Delta x}+\frac{v_{i+1/2,j}^{n+1}-v_{i-1/2,j}^{n+1}}{\Delta x} =0
$$
现在，我们需要用压力梯度来表示上式中的速度$u_{i+1/2,j}^{n+1}$，$u_{i-1/2,j}^{n+1}$，$v_{i,j+1/2}^{n+1}$，$v_{i,j-1/2}^{n+1}$

我们通过解偏微分方程$\frac{\vec{u}^{n+1}-\vec{u}^{n}}{\Delta t}+\frac{1}{\rho}\nabla p = 0$得到
$$
u_{i+1/2,j}^{n+1}=u_{i+1/2,j}-\Delta t \frac{1}{\rho}\frac{p_{i+1,j}-p_{i,j}}{\Delta x}\\
u_{i-1/2,j}^{n+1}=u_{i-1/2,j}-\Delta t \frac{1}{\rho}\frac{p_{i-1,j}-p_{i,j}}{\Delta x}\\
v_{i,j+1/2}^{n+1}=v_{i,j+1/2}-\Delta t \frac{1}{\rho}\frac{p_{i,j+1}-p_{i,j}}{\Delta x}\\
v_{i,j-1/2}^{n+1}=v_{i,j-1/2}-\Delta t \frac{1}{\rho}\frac{p_{i,j-1}-p_{i,j}}{\Delta x}
$$
把$u_{i+1/2,j}^{n+1}$，$u_{i-1/2,j}^{n+1}$，$v_{i,j+1/2}^{n+1}$，$v_{i,j-1/2}^{n+1}$代入式(8)，并整理，得到：
$$
\frac{\Delta t}{\rho}(\frac{4p_{i,j}-p_{i+1,j}-p_{i,j+1}-p_{i-1,j}-p_{i,j-1}}{\Delta x ^2})=-(\frac {u_{i+1/2, j}-u_{i-1/2,j}}{\Delta x},\frac {v_{i+1/2, j}-v_{i-1/2,j}}{\Delta x})
$$
这就是我们最终要求解的压力方程。

